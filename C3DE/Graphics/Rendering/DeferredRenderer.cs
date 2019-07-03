using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// A very **Work In Progress** Deferred Renderer
    /// </summary>
    public class DeferredRenderer : BaseRenderer
    {
        private QuadRenderer m_QuadRenderer;
        private RenderTarget2D m_ColorTarget;
        private RenderTarget2D m_DepthTarget;
        private RenderTarget2D m_NormalTarget;
        private RenderTarget2D m_LightTarget;
        private Effect m_ClearEffect;
        private Effect m_CombineEffect;

        public RenderTarget2D ColorBuffer => m_ColorTarget;
        public RenderTarget2D NormalMap => m_NormalTarget;
        public RenderTarget2D DepthBuffer => m_DepthTarget;
        public RenderTarget2D LightMap => m_LightTarget;

        public DeferredRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
            m_QuadRenderer = new QuadRenderer(graphics);
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            m_ClearEffect = content.Load<Effect>("Shaders/Deferred/Clear");
            m_CombineEffect = content.Load<Effect>("Shaders/Deferred/Combine");
        }

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
            if (!m_IsDisposed)
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
                m_IsDisposed = true;
            }
        }

        private void RenderObjects(Scene scene, Camera camera)
        {
            using (_graphicsDevice.GeometryUnlitState())
                if (scene.RenderSettings.Skybox.Enabled)
                    scene.RenderSettings.Skybox.Draw(_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            Renderer renderer;
            Material material;
            ShaderMaterial shader;

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
            }
        }

        private void RenderLights(Scene scene, Camera camera, int eye)
        {
            _graphicsDevice.SetRenderTargets(null);
            _graphicsDevice.SetRenderTarget(m_LightTarget);
            _graphicsDevice.Clear(Color.Transparent);

            m_AmbientLight.Color = Scene.current.RenderSettings.AmbientColor;
            m_AmbientLight.RenderDeferred(m_ColorTarget, m_NormalTarget, m_DepthTarget, camera);

            foreach (var light in scene.lights)
                light.RenderDeferred(m_ColorTarget, m_NormalTarget, m_DepthTarget, camera);
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, int eye)
        {
            _graphicsDevice.SetRenderTargets(m_ColorTarget, m_NormalTarget, m_DepthTarget);

            foreach (var pass in m_ClearEffect.Techniques[0].Passes)
            {
                pass.Apply();
                m_QuadRenderer.RenderFullscreenQuad();
            }

            using (_graphicsDevice.GeometryState())
                RenderObjects(scene, camera);

            using (_graphicsDevice.LightState())
                RenderLights(scene, camera, eye);

            _graphicsDevice.SetRenderTarget(_sceneRenderTargets[eye]);
            _graphicsDevice.Clear(Color.Black);

            using (_graphicsDevice.PostProcessState())
            {
                foreach (var pass in m_CombineEffect.Techniques[0].Passes)
                {
                    m_CombineEffect.Parameters["ColorMap"].SetValue(m_ColorTarget);
                    m_CombineEffect.Parameters["LightMap"].SetValue(m_LightTarget);
                    pass.Apply();
                    m_QuadRenderer.RenderFullscreenQuad();
                }

                RenderPostProcess(scene.postProcessPasses, _sceneRenderTargets[eye]);

                if (!m_VREnabled)
                {
                    RenderToBackBuffer();
                    RenderUI(scene.Behaviours);
                }
            }
        }

        public override void Render(Scene scene)
        {
            if (scene == null || scene?.cameras.Count == 0)
                return;

            var camera = scene.cameras[0];

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
                    camera._projectionMatrix = m_VRService.GetProjectionMatrix(eye);
                    camera._viewMatrix = m_VRService.GetViewMatrix(eye, cameraParent);
                    RenderSceneForCamera(scene, camera, eye);
                }

                m_VRService.SubmitRenderTargets(_sceneRenderTargets[0], _sceneRenderTargets[1]);
                DrawVRPreview(0);
                RenderUI(scene.Behaviours);
            }
            else
                RenderSceneForCamera(scene, camera, 0);
        }

        public override void RenderReflectionProbe(Camera camera)
        {
        }
    }
}
