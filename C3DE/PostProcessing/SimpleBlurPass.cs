using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcessing
{
    public class SimpleBlurPass : PostProcessPass
    {
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;

        public float BlurDistance { get; set; } = 0;

        public SimpleBlurPass(GraphicsDevice graphics)
             : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/SimpleBlur");
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            m_Effect.Parameters["BlurDistance"].SetValue(BlurDistance);

            DrawFullscreenQuad(spriteBatch, sceneRT, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            var viewport = m_GraphicsDevice.Viewport;
            m_GraphicsDevice.SetRenderTarget(sceneRT);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
