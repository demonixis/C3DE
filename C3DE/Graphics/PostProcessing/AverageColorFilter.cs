using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public sealed class AverageColorFilter : PostProcessPass
    {
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;

        public AverageColorFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/AverageColor");
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            _graphics.SetRenderTarget(m_SceneRenderTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;

            DrawFullscreenQuad(spriteBatch, renderTarget, m_SceneRenderTarget, m_Effect);

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = m_SceneRenderTarget;

            var viewport = _graphics.Viewport;
            _graphics.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
