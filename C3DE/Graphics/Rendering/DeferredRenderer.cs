using C3DE.Components;
using C3DE.Graphics.Materials;
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
    public class DeferredRenderer : Renderer
    {
        private QuadRenderer m_QuadRenderer;
        private RenderTarget2D m_ColorTarget;
        private RenderTarget2D m_DepthTarget;
        private RenderTarget2D m_NormalTarget;
        private RenderTarget2D m_LightTarget;
        private RenderTarget2D m_SceneFinalRT;
        private Effect m_ClearEffect;
        private Effect m_CombineEffect;
        private Effect m_TempRenderEffect;

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
            m_SceneFinalRT = new RenderTarget2D(m_graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
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

            using (m_graphicsDevice.GeometryState())
            {
                using (m_graphicsDevice.GeometryUnlitState())
                    if (scene.RenderSettings.Skybox.Enabled)
                        scene.RenderSettings.Skybox.DrawDeferred(m_graphicsDevice, camera);

                foreach (var renderer in scene.renderList)
                {
                    var material = renderer.material;
                    if (material == null)
                        continue;

                    // FIXME: Materials have to be updated.
                    m_TempRenderEffect.Parameters["World"].SetValue(renderer.m_Transform.m_WorldMatrix);
                    m_TempRenderEffect.Parameters["View"].SetValue(camera.m_ViewMatrix);
                    m_TempRenderEffect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
                    m_TempRenderEffect.Parameters["Texture"].SetValue(renderer.material.MainTexture);
                    m_TempRenderEffect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
                    m_TempRenderEffect.Parameters["DiffuseColor"].SetValue(material.m_DiffuseColor);

                    if (material is StandardMaterial)
                    {
                        var standard = (StandardMaterial)material;
                        m_TempRenderEffect.Parameters["NormalTextureEnabled"].SetValue(standard.NormalTexture != null);
                        m_TempRenderEffect.Parameters["NormalMap"].SetValue(standard.NormalTexture);
                        m_TempRenderEffect.Parameters["SpecularTextureEnabled"].SetValue(standard.SpecularTexture != null);
                        m_TempRenderEffect.Parameters["SpecularMap"].SetValue(standard.SpecularTexture);
                    }

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

            m_graphicsDevice.SetRenderTarget(m_SceneFinalRT);
            m_graphicsDevice.Clear(Color.Black);

            using (m_graphicsDevice.PostProcessState())
            {
                foreach (var pass in m_CombineEffect.Techniques[0].Passes)
                {
                    m_CombineEffect.Parameters["ColorMap"].SetValue(m_ColorTarget);
                    m_CombineEffect.Parameters["LightMap"].SetValue(m_LightTarget);
                    pass.Apply();
                    m_QuadRenderer.RenderFullscreenQuad(m_graphicsDevice);
                }

                RenderPostProcess(scene.postProcessPasses, m_SceneFinalRT);
            }

           
            RenderBuffers();
            RenderUI(scene.Behaviours);
        }

        protected virtual void RenderBuffers()
        {
            m_graphicsDevice.SetRenderTarget(null);
            m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            m_spriteBatch.Draw(m_SceneFinalRT, Vector2.Zero, Color.White);
            m_spriteBatch.End();
        }

        /// <summary>
        /// Renders effects.
        /// </summary>
        /// <param name="passes"></param>
        /// <param name="renderTarget"></param>
        protected void RenderPostProcess(List<PostProcessPass> passes, RenderTarget2D renderTarget)
        {
            if (passes.Count == 0)
                return;

            m_graphicsDevice.SetRenderTarget(renderTarget);

            for (int i = 0, l = passes.Count; i < l; i++)
                if (passes[i].Enabled)
                    passes[i].Draw(m_spriteBatch, renderTarget);
        }

        public override void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
        }
    }
}
