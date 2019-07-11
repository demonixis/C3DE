using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Materials.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public class ForwardRenderer : BaseRenderer
    {
        private DepthRenderer _depthRenderer;

        public DepthRenderer DepthRenderer => _depthRenderer;

        public ForwardRenderer(GraphicsDevice graphics)
           : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            RebuildRenderTargets();
            _depthRenderer = new DepthRenderer();
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
            return _depthRenderer._depthRT;
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
            if (scene == null || scene?.cameras.Count == 0)
                return;

            var camera = scene.cameras[0];

#if ANDROID
            _graphicsDevice.Clear(camera._clearColor);

            RenderObjects(scene, camera);
            RenderUI(scene.Behaviours);
            return;
#endif

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
                RenderUI(scene.Behaviours);
            }
            else
                RenderSceneForCamera(scene, camera, _sceneRenderTargets[0]);
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, RenderTarget2D renderTarget)
        {
            if (_depthRenderer.Enabled)
                _depthRenderer.Draw(_graphicsDevice);

            _graphicsDevice.SetRenderTarget(renderTarget);

            var renderToRT = camera.RenderTarget != null;
            if (renderToRT)
                _graphicsDevice.SetRenderTarget(camera.RenderTarget);

            _graphicsDevice.Clear(camera._clearColor);

            RenderObjects(scene, camera);
            RenderPostProcess(scene.postProcessPasses, renderTarget);

            if (renderToRT)
                return;

            if (!m_VREnabled)
            {
                RenderToBackBuffer();
                RenderUI(scene.Behaviours);
            }
        }

        /// <summary>
        /// Renders renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        protected void RenderObjects(Scene scene, Camera camera)
        {
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.BlendState = BlendState.Opaque;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            Renderer renderer;
            Material material;
            ShaderMaterial shader;
            IMultipassLightingMaterial multiLightShader;
            var lights = scene.lights;
            var lightCount = lights.Count;

            // Pass, Update matrix, material attributes, etc.
            for (var i = 0; i < renderCount; i++)
            {
                renderer = scene.renderList[i];
                material = scene.renderList[i].Material;

                // A specific renderer that uses its own draw logic.
                if (material == null)
                {
                    renderer.Draw(_graphicsDevice);
                    continue;
                }

                shader = material._shaderMaterial;

                // Ambient pass
                shader.PrePass(camera);
                shader.Pass(scene.RenderList[i]);
                renderer.Draw(_graphicsDevice);

                // Lightpass
                // Deprecated.
                if (shader is IMultipassLightingMaterial)
                {
                    multiLightShader = (IMultipassLightingMaterial)shader;

                    _graphicsDevice.BlendState = BlendState.Additive;

                    for (var l = 0; l < lightCount; l++)
                    {
                        multiLightShader.LightPass(renderer, lights[l]);
                        renderer.Draw(_graphicsDevice);
                    }

                    _graphicsDevice.BlendState = BlendState.Opaque;
                }
            }
        }
    }
}
