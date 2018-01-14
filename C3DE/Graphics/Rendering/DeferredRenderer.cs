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
        private RenderTarget2D[] m_ColorTarget;
        private RenderTarget2D[] m_DepthTarget;
        private RenderTarget2D[] m_NormalTarget;
        private RenderTarget2D[] m_LightTarget;
        private Effect m_ClearEffect;
        private Effect m_CombineEffect;

        public RenderTarget2D ColorBuffer => m_ColorTarget[0];
        public RenderTarget2D NormalMap => m_NormalTarget[0];
        public RenderTarget2D DepthBuffer => m_DepthTarget[0];
        public RenderTarget2D LightMap => m_LightTarget[0];

        public DeferredRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
            m_QuadRenderer = new QuadRenderer(graphics);
            m_ColorTarget = new RenderTarget2D[2];
            m_DepthTarget = new RenderTarget2D[2];
            m_NormalTarget = new RenderTarget2D[2];
            m_LightTarget = new RenderTarget2D[2];
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

            for (var i = 0; i < 2; i++)
            {
                // Do not create secondary render targets if VR is not enabled.
                if (i > 0 && !m_VREnabled)
                    continue;

                m_ColorTarget[i] = CreateRenderTarget(SurfaceFormat.Color);
                m_NormalTarget[i] = CreateRenderTarget(SurfaceFormat.Color);
                m_DepthTarget[i] = CreateRenderTarget(SurfaceFormat.Single);
                m_LightTarget[i] = CreateRenderTarget(SurfaceFormat.Color);
            }
        }

        public override void Dispose(bool disposing)
        {
            if (!m_IsDisposed)
            {
                if (disposing)
                {
                    for (var eye = 0; eye < 2; eye++)
                        DisposeObject(m_SceneRenderTargets[eye]);

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
            using (m_graphicsDevice.GeometryUnlitState())
                if (scene.RenderSettings.Skybox.Enabled)
                    scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

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
                    renderer.Draw(m_graphicsDevice);
                    continue;
                }

                shader = material.m_ShaderMaterial;

                // Ambient pass
                shader.PrePass(camera);
                shader.Pass(scene.RenderList[i]);
                renderer.Draw(m_graphicsDevice);
            }
        }

        private void RenderLights(Scene scene, Camera camera, int eye)
        {
            m_graphicsDevice.SetRenderTargets(null);
            m_graphicsDevice.SetRenderTarget(m_LightTarget[eye]);
            m_graphicsDevice.Clear(Color.Transparent);

            foreach (var light in scene.lights)
                light.RenderDeferred(m_ColorTarget[eye], m_NormalTarget[eye], m_DepthTarget[eye], camera);
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, int eye)
        {
            m_graphicsDevice.SetRenderTargets(m_ColorTarget[eye], m_NormalTarget[eye], m_DepthTarget[eye]);

            foreach (var pass in m_ClearEffect.Techniques[0].Passes)
            {
                pass.Apply();
                m_QuadRenderer.RenderFullscreenQuad(m_graphicsDevice);
            }

            using (m_graphicsDevice.GeometryState())
                RenderObjects(scene, camera);

            using (m_graphicsDevice.LightState())
                RenderLights(scene, camera, eye);

            m_graphicsDevice.SetRenderTarget(m_SceneRenderTargets[eye]);
            m_graphicsDevice.Clear(Color.Black);

            using (m_graphicsDevice.PostProcessState())
            {
                foreach (var pass in m_CombineEffect.Techniques[0].Passes)
                {
                    m_CombineEffect.Parameters["ColorMap"].SetValue(m_ColorTarget[eye]);
                    m_CombineEffect.Parameters["LightMap"].SetValue(m_LightTarget[eye]);
                    pass.Apply();
                    m_QuadRenderer.RenderFullscreenQuad(m_graphicsDevice);
                }

                RenderPostProcess(scene.postProcessPasses, m_SceneRenderTargets[eye]);

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
            var cameraParent = Matrix.Identity;
            var parent = camera.m_Transform.Parent;
            if (parent != null)
                cameraParent = parent.m_WorldMatrix;

            RebuildRenderTargets();

            RenderShadowMaps(scene);

            if (m_VREnabled)
            {
                for (var eye = 0; eye < 2; eye++)
                {
                    camera.m_ProjectionMatrix = m_VRService.GetProjectionMatrix(eye);
                    camera.m_ViewMatrix = m_VRService.GetViewMatrix(eye, cameraParent);
                    RenderSceneForCamera(scene, camera, eye);
                }

                m_VRService.SubmitRenderTargets(m_SceneRenderTargets[0], m_SceneRenderTargets[1]);
                DrawVRPreview(0);
                RenderUI(scene.Behaviours);
            }
            else
                RenderSceneForCamera(scene, camera, 0);
        }
    }
}
