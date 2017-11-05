using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Rendering.PostProcessing
{
    public class Refraction : PostProcessPass
    {
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;
        private Texture2D m_RefractionTexture;

        public float ColorLevel { get; set; } = 0.5f;
        public float Depth { get; set; } = 0.5f;
        public Vector2 TextureTiling { get; set; } = Vector2.One;

        public Texture2D RefractionTexture
        {
            get { return m_RefractionTexture; }
            set { m_RefractionTexture = value; }
        }

        public Refraction(GraphicsDevice graphics)
                : base(graphics)
        {
        }

        public Refraction(GraphicsDevice graphics, Texture2D refractionTexture)
                : base(graphics)
        {
            m_RefractionTexture = refractionTexture;
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/Refraction");
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            m_Effect.Parameters["RefractionTexture"].SetValue(m_RefractionTexture);
            m_Effect.Parameters["ColorLevel"].SetValue(ColorLevel);
            m_Effect.Parameters["Depth"].SetValue(Depth);

            DrawFullscreenQuad(spriteBatch, sceneRT, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            var viewport = m_GraphicsDevice.Viewport;
            m_GraphicsDevice.SetRenderTarget(sceneRT);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
