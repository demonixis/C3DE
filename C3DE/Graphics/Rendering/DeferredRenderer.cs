using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// A very **Work In Progress** Deferred Renderer
    /// </summary>
    public class DeferredRenderer : BaseRenderer
    {
        private QuadRenderer _quadRenderer;
        private RenderTarget2D m_ColorTarget;
        private RenderTarget2D m_DepthTarget;
        private RenderTarget2D m_NormalTarget;
        private RenderTarget2D m_LightTarget;
        private Effect _clearEffect;
        private Effect _combineEffect;

        public RenderTarget2D ColorBuffer => m_ColorTarget;
        public RenderTarget2D NormalMap => m_NormalTarget;
        public RenderTarget2D DepthBuffer => m_DepthTarget;
        public RenderTarget2D LightMap => m_LightTarget;

        public DeferredRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
            _quadRenderer = new QuadRenderer(graphics);
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            _clearEffect = content.Load<Effect>("Shaders/Deferred/Clear");
            _combineEffect = content.Load<Effect>("Shaders/Deferred/Combine");
        }

        public override RenderTarget2D GetDepthBuffer() => m_DepthTarget;

        /// <summary>
        /// Rebuilds render targets if Dirty is true.
        /// NO VR Support during the initial implementation.
        /// </summary>
        protected override void RebuildRenderTargets()
        {
            if (!Dirty)
                return;

            base.RebuildRenderTargets();

            m_ColorTarget = CreateRenderTarget(SurfaceFormat.Color);
            m_NormalTarget = CreateRenderTarget(SurfaceFormat.Color);
            m_DepthTarget = CreateRenderTarget(SurfaceFormat.Single);
            m_LightTarget = CreateRenderTarget(SurfaceFormat.Color);
        }

        public override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    for (var eye = 0; eye < 2; eye++)
                        DisposeObject(_sceneRenderTargets[eye]);

                    DisposeObject(m_ColorTarget);
                    DisposeObject(m_NormalTarget);
                    DisposeObject(m_DepthTarget);
                    DisposeObject(m_LightTarget);
                }
                _disposed = true;
            }
        }

        private void RenderObjects(Scene scene, ref Vector3 cameraPosition, ref Matrix cameraViewMatrix, ref Matrix cameraProjectionMatrix)
        {
            if (scene.RenderSettings.Skybox.Enabled)
            {
                using (_graphicsDevice.GeometryUnlitState())
                    scene.RenderSettings.Skybox.Draw(_graphicsDevice, ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);
            }

            var renderCount = scene._renderList.Count;

            Renderer renderer;
            Material material;
            ShaderMaterial shader;

            for (var i = 0; i < renderCount; i++)
            {
                renderer = scene._renderList[i];
                material = scene._renderList[i].Material;

                // A specific renderer that uses its own draw logic.
                if (material == null)
                {
                    renderer.Draw(_graphicsDevice);
                    continue;
                }

                shader = material._shaderMaterial;
                shader.PrePass(ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);
                shader.Pass(ref renderer._transform._worldMatrix, renderer.ReceiveShadow, false);
                renderer.Draw(_graphicsDevice);
            }
        }

        private void RenderLights(Scene scene, Camera camera, int eye)
        {
            _graphicsDevice.SetRenderTargets(null);
            _graphicsDevice.SetRenderTarget(m_LightTarget);
            _graphicsDevice.Clear(Color.Transparent);

            // TODO: Make a LightRenderer that renders lights with data only
            _ambientLight.Color = Scene.current.RenderSettings.AmbientColor;
            _ambientLight.RenderDeferred(m_ColorTarget, m_NormalTarget, m_DepthTarget, camera);

            foreach (var light in scene._lights)
                light.RenderDeferred(m_ColorTarget, m_NormalTarget, m_DepthTarget, camera);
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, int eye)
        {
            var cameraPosition = camera._transform.Position;
            var cameraViewMatrix = camera._viewMatrix;
            var cameraProjectionMatrix = camera._projectionMatrix;

            _graphicsDevice.SetRenderTargets(m_ColorTarget, m_NormalTarget, m_DepthTarget);

            _clearEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderFullscreenQuad();

            using (_graphicsDevice.GeometryState())
                RenderObjects(scene, ref cameraPosition, ref cameraViewMatrix, ref cameraProjectionMatrix);

            using (_graphicsDevice.LightState())
                RenderLights(scene, camera, eye);
            
            _graphicsDevice.SetRenderTarget(_sceneRenderTargets[eye]);
            _graphicsDevice.Clear(Color.Black);

            using (_graphicsDevice.PostProcessState())
            {
                _combineEffect.Parameters["ColorMap"].SetValue(m_ColorTarget);
                _combineEffect.Parameters["LightMap"].SetValue(m_LightTarget);
                _combineEffect.CurrentTechnique.Passes[0].Apply();
                _quadRenderer.RenderFullscreenQuad();

                RenderPostProcess(scene._postProcessPasses, _sceneRenderTargets[eye]);

                if (!m_VREnabled)
                {
                    RenderToBackBuffer();
                    RenderUI(scene._scripts);
                }
            }
        }

        public override void Render(Scene scene)
        {
            if (scene == null || scene?._cameras.Count == 0)
                return;

            var camera = scene._cameras[0];

            RebuildRenderTargets();
            RenderShadowMaps(scene);

            if (m_VREnabled)
            {
                var cameraParent = Matrix.Identity;
                var parent = camera._transform.Parent;
                if (parent != null)
                    cameraParent = parent._worldMatrix;

                for (var eye = 0; eye < 2; eye++)
                {
                    camera._projectionMatrix = _VRService.GetProjectionMatrix(eye);
                    camera._viewMatrix = _VRService.GetViewMatrix(eye, cameraParent);
                    RenderSceneForCamera(scene, camera, eye);
                }

                _VRService.SubmitRenderTargets(_sceneRenderTargets[0], _sceneRenderTargets[1]);
                DrawVRPreview(0);
                RenderUI(scene._scripts);
            }
            else
                RenderSceneForCamera(scene, camera, 0);
        }
    }
}
