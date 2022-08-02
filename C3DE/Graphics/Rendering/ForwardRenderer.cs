using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering.Passes;
using C3DE.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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

        // Render Passes
        private DepthPass _depthPass;
        private NormalPass _normalPass;
        private DepthNormalPass _depthNormalPass;

        // Lighting
        private List<int> _culledLights;
        private LightData _lightData;
        private ShadowData _shadowData;

        public bool DirectRendering { get; set; }
        public bool UseMRT { get; set; } = false;

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
            _culledLights = new List<int>(MaxLightLimit);
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            RebuildRenderTargets();

            _depthPass = new DepthPass(_graphicsDevice);
            _depthPass.LoadContent(content);

            _normalPass = new NormalPass(_graphicsDevice);
            _normalPass.LoadContent(content);

            _depthNormalPass = new DepthNormalPass(_graphicsDevice);
            _depthNormalPass.LoadContent(content);
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

        public override RenderTarget2D GetDepthBuffer()
        {
            _depthPass.Enabled = true;

            if (UseMRT && _normalPass.Enabled)
            {
                _depthNormalPass.Enabled = true;
                return _depthNormalPass.DepthBuffer;
            }

            _depthPass.Enabled = true;
            return _depthPass.RenderTarget;
        }

        public override RenderTarget2D GetNormalBuffer()
        {
            _normalPass.Enabled = true;

            if (UseMRT && _depthPass.Enabled)
            {
                _depthNormalPass.Enabled = true;
                return _depthNormalPass.NormalBuffer;
            }

            _normalPass.Enabled = true;
            return _normalPass.RenderTarget;
        }

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

            Camera camera = null;

            foreach (var cam in scene._cameras)
            {
                if (cam.RenderTarget == null)
                {
                    camera = cam;
                    break;
                }
            }

            if (camera == null)
            {
                RenderUI(scene._scripts);
                return;
            }

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

            // Use MRT
            if (!_depthNormalPass.Enabled)
            {
                if (_depthPass.Enabled)
                    _depthPass.Render(scene, camera);
                if (_normalPass.Enabled)
                    _normalPass.Render(scene, camera);
            }
           else
                _depthNormalPass.Render(scene, camera);

            // Render Reflection Probes.
            var reflectionProbes = scene._reflectionProbes;

            if (reflectionProbes.Count > 0)
            {
                Color[] colorBuffer = null;
                var oldRT = _graphicsDevice.GetRenderTargets();

                // TODO: Render
                foreach (var probe in reflectionProbes)
                {
                    var probeCamPos = Vector3.Zero;
                    var probeCamView = Matrix.Identity;
                    var probeCamProj = Matrix.Identity;
                    Camera probeCam = null;

                    if (colorBuffer == null || colorBuffer.Length != probe.Resolution)
                        colorBuffer = new Color[probe.Resolution * probe.Resolution];

                    if (probe.Mode == ReflectionProbe.RenderingMode.Realtime || probe.Dirty)
                    {
                        probeCam = probe._camera;

                        for (var i = 0; i < 6; i++)
                        {
                            probeCam._transform.LocalRotation = probe.GetCameraRotation((CubeMapFace)i);
                            probeCam._transform.UpdateWorldMatrix();
                            probeCam.Update();

                            probeCamPos = probeCam._transform.Position;
                            probeCamView = probeCam._viewMatrix;
                            probeCamProj = probeCam._projectionMatrix;

                            _graphicsDevice.SetRenderTarget(probeCam.RenderTarget);
                            _graphicsDevice.Clear(Color.Black);

                            RenderObjects(scene, ref probeCamPos, ref probeCamView, ref probeCamProj);
                            probeCam.RenderTarget.GetData(colorBuffer);

                            probe._reflectionTexture.SetData((CubeMapFace)i, colorBuffer);
                        }

                        probe.Dirty = false;
                    }
                }

                _graphicsDevice.SetRenderTargets(oldRT);
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
        protected void RenderObjects(Scene scene, ref Vector3 cameraPosition, ref Matrix cameraViewMatrix, ref Matrix cameraProjectionMatrix)
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            // Camera
            var fogData = scene.RenderSettings.fogData;

            ComputeLightData(scene);

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

                if (!renderer.Enabled || !renderer._gameObject.Enabled)
                    continue;

                material = scene._renderList[i].Material;

                // A specific renderer that uses its own draw logic.
                if (material == null)
                {
                    renderer.Draw(_graphicsDevice);
                    continue;
                }

                shader = material._shaderMaterial;
                shader.PrePassForward(ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix, ref _lightData, ref _shadowData, ref fogData);
                shader.Pass(ref renderer._transform._worldMatrix, renderer.ReceiveShadow, renderer.InstancedEnabled);

                renderer.Draw(_graphicsDevice);
            }
        }

        private void ComputeLightData(Scene scene)
        {
            // TODO: Put it in a cache.
            var lights = scene._lights;

            if (lights.Count != _lightData.Count)
            {
                lights.Sort();
            }

            // Only visible lights.
            _culledLights.Clear();

            for (var i = 0; i < lights.Count; i++)
            {
                if (lights[i].Enabled && lights[i].GameObject.Enabled)
                    _culledLights.Add(i);
            }

            var lightCount = Math.Min(MaxLightCount, _culledLights.Count);

            if (_lightData.Count != lightCount)
            {
                _lightData.Count = lightCount;
                _lightData.Colors = new Vector3[lightCount];
                _lightData.Positions = new Vector3[lightCount];
                _lightData.Data = new Vector4[lightCount];
                _lightData.SpotData = new Vector4[lightCount];
            }

            var shadow = false;
            Light light;
            for (var i = 0; i < lightCount; i++)
            {
                light = lights[_culledLights[i]];

                _lightData.Positions[i] = light.Transform.Position;
                _lightData.Colors[i] = light._color;
                _lightData.Data[i] = new Vector4(0, light.Intensity, light.Radius, light.FallOf);
                _lightData.SpotData[i] = new Vector4(light.Direction, light.Angle);

                if (light.Type == LightType.Point)
                    _lightData.Data[i].X = 1;
                else if (light.Type == LightType.Spot)
                    _lightData.Data[i].X = 2;

                if (!shadow && light.ShadowEnabled)
                {
                    _shadowData.ProjectionMatrix = light._projectionMatrix;
                    _shadowData.ViewMatrix = light._viewMatrix;
                    _shadowData.Data = light._shadowGenerator._shadowData;
                    _shadowData.ShadowMap = light._shadowGenerator.ShadowMap;
                    shadow = true;
                }
            }

            _lightData.Ambient = scene.RenderSettings.ambientColor;
        }
    }
}
