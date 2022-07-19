using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public sealed class Vignette : PostProcessPass
    {
        private Vector2 _viewportSize;

        public Vector2 Scale { get; set; } = new Vector2(1.5f);
        public float Power { get; set; } = 0.5f;

        public Vignette(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            _effect = content.Load<Effect>("Shaders/PostProcessing/Vignette");
        }

        public override void SetupEffect()
        {
            _effect.Parameters["ViewportSize"].SetValue(_viewportSize);
            _effect.Parameters["Scale"].SetValue(Scale);
            _effect.Parameters["Power"].SetValue(Power);
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            _viewportSize.X = sceneRT.Width;
            _viewportSize.Y = sceneRT.Height;
            base.Draw(spriteBatch, sceneRT);
        }
    }
}
