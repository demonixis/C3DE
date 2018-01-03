using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public sealed class Vignette : PostProcessPass
    {
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;
        private Vector2 m_ViewportSize;

        public Vector2 Scale { get; set; } = Vector2.One;
        public float Power { get; set; } = 1.0f;

        public Vignette(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/Vignette");
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            m_ViewportSize.X = renderTarget.Width;
            m_ViewportSize.Y = renderTarget.Height;

            m_Effect.Parameters["ViewportSize"].SetValue(m_ViewportSize);
            m_Effect.Parameters["Scale"].SetValue(Scale);
            m_Effect.Parameters["Power"].SetValue(Power);

            DrawFullscreenQuad(spriteBatch, renderTarget, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            var viewport = m_GraphicsDevice.Viewport;
            m_GraphicsDevice.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
