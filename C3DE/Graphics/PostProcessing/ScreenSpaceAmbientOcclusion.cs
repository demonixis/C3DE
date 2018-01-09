using C3DE.Components;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public sealed class ScreenSpaceAmbientOcclusion : PostProcessPass
    {
        private QuadRenderer m_QuadRenderer;
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;
        private RenderTarget2D m_DepthBuffer;
        private RenderTarget2D m_NormalBuffer;

        public float radius = 0.5f;
        public float intensity = 0.75f;
        public float scale = 0.2f;
        public float bias = 0.0001f;
        public Texture2D RandomTexture { get; set; }

        public float BlurDistance { get; set; } = 0;

        public ScreenSpaceAmbientOcclusion(GraphicsDevice graphics)
             : base(graphics)
        {
            m_QuadRenderer = new QuadRenderer(graphics);
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/SSAmbientOcclusion");
            m_SceneRenderTarget = GetRenderTarget();

            var renderer = Application.Engine.Renderer;
            if (renderer is ForwardRenderer)
            {
                var forward = (ForwardRenderer)renderer;
                forward.DepthRenderer.Enabled = true;
                m_DepthBuffer = forward.DepthRenderer.m_DepthRT;
                // FIXME
            }
            else if (renderer is DeferredRenderer)
            {
                var forward = (DeferredRenderer)renderer;
                m_DepthBuffer = forward.DepthBuffer;
                m_NormalBuffer = forward.NormalMap;
            }
            else if (renderer is LightPrePassRenderer)
            {
                var lpp = (LightPrePassRenderer)renderer;
                m_DepthBuffer = lpp.DepthBuffer;
                m_NormalBuffer = lpp.NormalBuffer;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            m_Effect.Parameters["HalfPixel"].SetValue(new Vector2(0.5f / Screen.Width, 0.5f / Screen.Height));
            m_Effect.Parameters["Radius"].SetValue(radius);
            m_Effect.Parameters["Intensity"].SetValue(intensity);
            m_Effect.Parameters["Scale"].SetValue(scale);
            m_Effect.Parameters["Bias"].SetValue(bias);
            m_Effect.Parameters["RandomMap"].SetValue(RandomTexture);

            var camera = Camera.Main;

            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
            m_Effect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(camera.m_ViewMatrix * camera.m_ProjectionMatrix));
            m_Effect.Parameters["InvertProjection"].SetValue(Matrix.Invert(camera.m_ProjectionMatrix));

            m_Effect.Parameters["NormalMap"].SetValue(m_NormalBuffer);
            m_Effect.Parameters["DepthBuffer"].SetValue(m_DepthBuffer);

            m_Effect.CurrentTechnique.Passes[0].Apply();

            DrawFullscreenQuad(spriteBatch, sceneRT, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            var viewport = m_GraphicsDevice.Viewport;
            m_GraphicsDevice.SetRenderTarget(sceneRT);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
