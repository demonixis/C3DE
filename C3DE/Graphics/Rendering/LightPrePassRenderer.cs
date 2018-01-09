using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Graphics.Rendering
{
    public class LightPrePassRenderer : Renderer
    {
        private QuadRenderer m_QuadRenderer;
        private Vector2 m_Viewport;
        internal RenderTarget2D m_DepthRT;
        internal RenderTarget2D m_NormalRT;
        internal RenderTarget2D m_LightRT;
        private RenderTarget2D m_SceneFinalRT;
        private Effect m_DepthNormalEffect;
        private Effect m_TempRenderEffect;

        public RenderTarget2D DepthBuffer => m_DepthRT;
        public RenderTarget2D NormalBuffer => m_NormalRT;
        public RenderTarget2D LightBuffer => m_LightRT;

        public LightPrePassRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
            m_QuadRenderer = new QuadRenderer(graphics);
            m_Viewport.X = m_graphicsDevice.Viewport.Width;
            m_Viewport.Y = m_graphicsDevice.Viewport.Height;
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            m_Viewport.X = m_graphicsDevice.Viewport.Width;
            m_Viewport.Y = m_graphicsDevice.Viewport.Height;
            m_DepthNormalEffect = content.Load<Effect>("Shaders/LPP/DepthNormal");
            m_TempRenderEffect = content.Load<Effect>("Shaders/LPP/Standard");
        }

        protected override void RebuildRenderTargets()
        {
            if (!Dirty)
                return;

            base.RebuildRenderTargets();

            var device = Application.GraphicsDevice;
            var width = m_graphicsDevice.PresentationParameters.BackBufferWidth;
            var height = m_graphicsDevice.PresentationParameters.BackBufferHeight;

            m_DepthRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Single, DepthFormat.Depth24);
            m_NormalRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            m_LightRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            m_SceneFinalRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public override void Dispose(bool disposing)
        {
            if (!m_IsDisposed)
            {
                if (disposing)
                {
                    for (var eye = 0; eye < 2; eye++)
                        DisposeObject(m_SceneRenderTargets[eye]);

                    DisposeObject(m_DepthRT);
                    DisposeObject(m_NormalRT);
                    DisposeObject(m_LightRT);
                    DisposeObject(m_SceneFinalRT);
                }
                m_IsDisposed = true;
            }
        }

        private void DrawDepthNormalMap(Scene scene, Camera camera)
        {
            m_graphicsDevice.SetRenderTargets(m_NormalRT, m_DepthRT);
            m_graphicsDevice.Clear(Color.White);

            m_DepthNormalEffect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_DepthNormalEffect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);

            for (int i = 0, l = scene.renderList.Count; i < l; i++)
            {
                m_DepthNormalEffect.Parameters["World"].SetValue(scene.renderList[i].Transform.m_WorldMatrix);
                m_DepthNormalEffect.CurrentTechnique.Passes[0].Apply();
                scene.renderList[i].Draw(m_graphicsDevice);
            }

            m_graphicsDevice.SetRenderTarget(null);
        }

        private void DrawLightMap(Scene scene, Camera camera)
        {
            m_graphicsDevice.SetRenderTarget(m_LightRT);
            m_graphicsDevice.Clear(Color.Black);

            for (var i = 0; i < scene.lights.Count; i++)
                scene.lights[i].RenderLPP(m_NormalRT, m_DepthRT, camera);

            m_graphicsDevice.SetRenderTarget(null);
        }

        private void DrawObjects(Scene scene, Camera camera)
        {
            m_graphicsDevice.SetRenderTarget(m_SceneFinalRT);
            m_graphicsDevice.Clear(camera.clearColor);

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

            foreach (var renderer in scene.renderList)
            {
                var material = renderer.material;
                if (material == null)
                    continue;

                // FIXME: Materials have to be updated.
                m_TempRenderEffect.Parameters["World"].SetValue(renderer.m_Transform.m_WorldMatrix);
                m_TempRenderEffect.Parameters["View"].SetValue(camera.m_ViewMatrix);
                m_TempRenderEffect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
                m_TempRenderEffect.Parameters["MainTexture"].SetValue(renderer.material.MainTexture);
                m_TempRenderEffect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
                m_TempRenderEffect.Parameters["DiffuseColor"].SetValue(material.m_DiffuseColor);
                m_TempRenderEffect.Parameters["LightMap"].SetValue(m_LightRT);
                m_TempRenderEffect.Parameters["Viewport"].SetValue(m_Viewport);
                m_TempRenderEffect.CurrentTechnique.Passes[0].Apply();
                renderer.Draw(m_graphicsDevice);
            }
        }

        protected void RenderToBackBuffer()
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

        public override void Render(Scene scene)
        {
            var camera = Camera.Main;

            RebuildRenderTargets();

            using (m_graphicsDevice.GeometryState())
                DrawDepthNormalMap(scene, camera);

            using (m_graphicsDevice.LightPrePassState())
                DrawLightMap(scene, camera);

            using (m_graphicsDevice.GeometryState())
                DrawObjects(scene, camera);

            using (m_graphicsDevice.PostProcessState())
            {
                RenderPostProcess(scene.postProcessPasses, m_SceneFinalRT);
                RenderToBackBuffer();
                RenderUI(scene.Behaviours);
            }
        }
    }
}

