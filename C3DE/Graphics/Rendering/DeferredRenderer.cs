using C3DE.Components;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// A very **Work In Progress** Deferred Renderer
    /// </summary>
    public class DeferredRenderer : Renderer
    {
        private QuadRenderer m_QuadRenderer;
        private RenderTarget2D m_ColorTarget;
        private RenderTarget2D m_DepthTarget;
        private RenderTarget2D m_NormalTarget;
        private RenderTarget2D m_LightTarget;
        private Effect m_ClearEffect;
        private Effect m_CombineEffect;
        private Effect m_TempRenderEffect;

        public Texture2D ColorBuffer => m_ColorTarget;
        public Texture2D NormalMap => m_NormalTarget;
        public Texture2D DepthBuffer => m_DepthTarget;
        public Texture2D LightMap => m_LightTarget;

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
            m_TempRenderEffect = content.Load<Effect>("Shaders/Deferred/Standard");
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

            var width = m_graphicsDevice.PresentationParameters.BackBufferWidth;
            var height = m_graphicsDevice.PresentationParameters.BackBufferHeight;

            m_ColorTarget = new RenderTarget2D(m_graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            m_NormalTarget = new RenderTarget2D(m_graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            m_DepthTarget = new RenderTarget2D(m_graphicsDevice, width, height, false, SurfaceFormat.Single, DepthFormat.None);
            m_LightTarget = new RenderTarget2D(m_graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
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

        public override void Render(Scene scene)
        {
            var camera = scene.cameras[0];

            RebuildRenderTargets();

            RenderShadowMaps(scene);

            m_graphicsDevice.SetRenderTargets(m_ColorTarget, m_NormalTarget, m_DepthTarget);

            foreach (var pass in m_ClearEffect.Techniques[0].Passes)
            {
                pass.Apply();
                m_QuadRenderer.RenderFullscreenQuad(m_graphicsDevice);
            }

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.DrawDeferred(m_graphicsDevice, camera);

            using (m_graphicsDevice.GeometryState())
            {
                foreach (var renderer in scene.renderList)
                {
                    // FIXME: Materials have to be updated.
                    m_TempRenderEffect.Parameters["World"].SetValue(renderer.m_Transform.m_WorldMatrix);
                    m_TempRenderEffect.Parameters["View"].SetValue(camera.m_ViewMatrix);
                    m_TempRenderEffect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
                    m_TempRenderEffect.Parameters["Texture"].SetValue(renderer.material.MainTexture);
                    m_TempRenderEffect.CurrentTechnique.Passes[0].Apply();
                    renderer.Draw(m_graphicsDevice);
                }
            }

            m_graphicsDevice.SetRenderTargets(null);
            m_graphicsDevice.SetRenderTarget(m_LightTarget);
            m_graphicsDevice.Clear(Color.Transparent);

            using (m_graphicsDevice.LightState())
            {
                foreach (var light in scene.lights)
                    light.RenderDeferred(m_ColorTarget, m_NormalTarget, m_DepthTarget, camera);
            }

            m_graphicsDevice.SetRenderTargets(null);

            using (m_graphicsDevice.PostProcessState())
            {
                foreach (var pass in m_CombineEffect.Techniques[0].Passes)
                {
                    m_CombineEffect.Parameters["ColorMap"].SetValue(m_ColorTarget);
                    m_CombineEffect.Parameters["LightMap"].SetValue(m_LightTarget);
                    pass.Apply();
                    m_QuadRenderer.RenderFullscreenQuad(m_graphicsDevice);
                }
            }

            RenderUI(scene.Behaviours);
        }

        public override void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
        }
    }
}
