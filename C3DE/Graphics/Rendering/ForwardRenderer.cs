using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public class ForwardRenderer : BaseRenderer
    {
#if WINDOWS
        internal const int MaxLightLimit = 128;
#else
        internal const int MaxLightLimit = 16;
#endif

        private static int _maxLightCount = MaxLightLimit;

        private RenderTarget2D _depthRT;
        private Effect _depthEffect;
        private LightData _lightData;
        private ShadowData _shadowData;

        public bool DirectRendering { get; set; } = false;
        public bool DepthPass { get; set; } = false;

        public static int MaxLightCount
        {
            get => _maxLightCount;
            set
            {
                _maxLightCount = value;

                if (_maxLightCount > MaxLightLimit)
                    _maxLightCount = MaxLightLimit;
            }
        }

        public ForwardRenderer(GraphicsDevice graphics)
           : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            RebuildRenderTargets();

            var pp = _graphicsDevice.PresentationParameters;
            _depthRT = new RenderTarget2D(_graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            _depthEffect = content.Load<Effect>("Shaders/Depth");
        }

        public override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    DisposeObject(_sceneRenderTargets);

                _disposed = true;
            }
        }

        public override RenderTarget2D GetDepthBuffer() => _depthRT;

        /// <summary>
        /// Renders the scene with the specified camera.
        /// Render order:
        /// 1 - Shadow maps
        /// 2 - Objects
        /// 3 - Post Processing
        /// 4 - UI
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        public override void Render(Scene scene)
        {
            if (scene == null || scene?._cameras.Count == 0)
                return;

            var camera = scene._cameras[0];

            if (DirectRendering)
            {
                _graphicsDevice.Clear(camera._clearColor);

                var cameraPosition = camera._transform.Position;
                var cameraViewMatrix = camera._viewMatrix;
                var cameraProjectionMatrix = camera._projectionMatrix;

                RenderObjects(scene, ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);
                RenderUI(scene._scripts);
            }
            else
            {
                RebuildRenderTargets();
                RenderShadowMaps(scene);

                if (m_VREnabled)
                {
                    // Apply camera parenting
                    var cameraParent = Matrix.Identity;
                    var parent = camera._transform.Parent;
                    if (parent != null)
                        cameraParent = parent._worldMatrix;

                    for (var eye = 0; eye < 2; eye++)
                    {
                        camera._projectionMatrix = _VRService.GetProjectionMatrix(eye);
                        camera._viewMatrix = _VRService.GetViewMatrix(eye, cameraParent);
                        RenderSceneForCamera(scene, camera, _sceneRenderTargets[eye]);
                    }

                    _VRService.SubmitRenderTargets(_sceneRenderTargets[0], _sceneRenderTargets[1]);
                    DrawVRPreview(0);
                    RenderUI(scene._scripts);
                }
                else
                    RenderSceneForCamera(scene, camera, _sceneRenderTargets[0]);
            }
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, RenderTarget2D renderTarget)
        {
            var cameraPosition = camera._transform.Position;
            var cameraViewMatrix = camera._viewMatrix;
            var cameraProjectionMatrix = camera._projectionMatrix;

            if (DepthPass)
            {
                var previousRTs = _graphicsDevice.GetRenderTargets();

                _graphicsDevice.SetRenderTarget(_depthRT);
                _graphicsDevice.Clear(Color.Transparent);

                _depthEffect.Parameters["View"].SetValue(cameraViewMatrix);
                _depthEffect.Parameters["Projection"].SetValue(cameraProjectionMatrix);

                var renderList = scene._renderList;

                for (int i = 0, l = renderList.Count; i < l; i++)
                {
                    if (renderList[i] is LensFlare)
                        continue;

                    _depthEffect.Parameters["World"].SetValue(renderList[i].Transform._worldMatrix);
                    _depthEffect.CurrentTechnique.Passes[0].Apply();
                    renderList[i].Draw(_graphicsDevice);
                }

                _graphicsDevice.SetRenderTargets(previousRTs);
            }

            // Render Planar Reflections.
            var planarReflections = scene._planarReflections;

            if (planarReflections.Count > 0)
            {
                cameraViewMatrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
                var cameraRotation = rotation.ToEuler();

                foreach (var planar in planarReflections)
                {
                    if (!planar.IsReady)
                        continue;

                    planar.BeginDraw(_graphicsDevice, ref cameraPosition, ref cameraRotation);

                    // Limit the number of lights.
                    RenderObjects(scene, ref planar._reflectionCameraPosition, ref planar._reflectionViewMatrix, ref cameraProjectionMatrix, planar);

                    planar.EndDraw(_graphicsDevice);
                }
            }

            // Render Reflection Probes.
            var reflectionProbes = scene._reflectionProbes;

            if (reflectionProbes.Count > 0)
            {
                // TODO: Render
                foreach (var probe in reflectionProbes)
                {

                }
            }

            // Render the scene
            _graphicsDevice.SetRenderTarget(renderTarget);

            var renderToRT = camera.RenderTarget != null;
            if (renderToRT)
                _graphicsDevice.SetRenderTarget(camera.RenderTarget);

            _graphicsDevice.Clear(camera._clearColor);

            RenderObjects(scene, ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);
            RenderPostProcess(scene._postProcessPasses, renderTarget);

            if (renderToRT)
                return;

            if (!m_VREnabled)
            {
                RenderToBackBuffer();
                RenderUI(scene._scripts);
            }
        }

        /// <summary>
        /// Renders renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        protected void RenderObjects(Scene scene, ref Vector3 cameraPosition, ref Matrix cameraViewMatrix, ref Matrix cameraProjectionMatrix, PlanarReflection planarReflection = null)
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            // Camera
            var fogData = scene.RenderSettings.fogData;

            ComputeLightData(scene, -1);

            if (scene.RenderSettings.Skybox.Enabled)
            {
                using (_graphicsDevice.GeometryState())
                    scene.RenderSettings.Skybox.Draw(_graphicsDevice, ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);
            }

            var renderCount = scene._renderList.Count;

            Renderer renderer;
            Material material;
            ShaderMaterial shader;

            // Pass, Update matrix, material attributes, etc.
            for (var i = 0; i < renderCount; i++)
            {
                renderer = scene._renderList[i];
                material = scene._renderList[i].Material;

                if (planarReflection != null)
                {
                    // Don't render this object if we're rendering the planar reflection attached to this object.
                    var valid = !renderer.IsUsingPlanarReflection(planarReflection);
                    valid &= !(renderer is LensFlare);

                    if (!valid)
                        continue;
                }

                // A specific renderer that uses its own draw logic.
                if (material == null)
                {
                    renderer.Draw(_graphicsDevice);
                    continue;
                }

                shader = material._shaderMaterial;
                shader.PrePass(ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix, ref _lightData, ref _shadowData, ref fogData);

                // FIXME: More cache..
                if (renderer.PlanarReflection != null)
                {
                    var reflectionView = shader._effect.Parameters["ReflectionView"];
                    var reflectionMap = shader._effect.Parameters["ReflectionMap"];

                    if (reflectionView != null && reflectionMap != null)
                    {
                        reflectionView.SetValue(renderer.PlanarReflection._reflectionViewMatrix);
                        reflectionMap.SetValue(renderer.PlanarReflection._reflectionRT);
                    }
                }

                shader.Pass(ref renderer._transform._worldMatrix, renderer.ReceiveShadow, renderer.InstancedEnabled);

                renderer.Draw(_graphicsDevice);
            }
        }

        private void ComputeLightData(Scene scene, int limit)
        {
            // TODO: Put it in a cache.
            var lights = scene._lights;
            var lightCount = lights.Count;

            lightCount = Math.Min(MaxLightCount, lightCount);

            if (lightCount > 1 && _lightData.Count == 0)
            {
                lights.Sort();
            }

            if (_lightData.Count != lightCount)
            {
                _lightData.Count = lightCount;
                _lightData.Colors = new Vector3[lightCount];
                _lightData.Positions = new Vector3[lightCount];
                _lightData.Data = new Vector4[lightCount];
                _lightData.SpotData = new Vector4[lightCount]; 
            }

            if (limit > -1)
                _lightData.Count = limit;

            var shadow = false;

            for (var i = 0; i < lightCount; i++)
            {
                _lightData.Positions[i] = lights[i].Transform.Position;
                _lightData.Colors[i] = lights[i]._color;
                _lightData.Data[i] = new Vector4(0, lights[i].Intensity, lights[i].Radius, lights[i].FallOf);
                _lightData.SpotData[i] = new Vector4(lights[i].Direction, lights[i].Angle);

                if (lights[i].Type == LightType.Point)
                    _lightData.Data[i].X = 1;
                else if (lights[i].Type == LightType.Spot)
                    _lightData.Data[i].X = 2;

                if (!shadow && lights[i].ShadowEnabled)
                {
                    _shadowData.ProjectionMatrix = lights[i]._projectionMatrix;
                    _shadowData.ViewMatrix = lights[i]._viewMatrix;
                    _shadowData.Data = lights[i]._shadowGenerator._shadowData;
                    _shadowData.ShadowMap = lights[i]._shadowGenerator.ShadowMap;
                    shadow = true;
                }
            }

            _lightData.Ambient = scene.RenderSettings.ambientColor;
        }
    }
}
