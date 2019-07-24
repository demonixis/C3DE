using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public sealed class AverageColorFilter : PostProcessPass
    {
        private Effect _effect;
        private RenderTarget2D _sceneRenderTarget;

        public AverageColorFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/PostProcessing/AverageColor");
            _sceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            _graphics.SetRenderTarget(_sceneRenderTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;

            DrawFullscreenQuad(spriteBatch, renderTarget, _sceneRenderTarget, _effect);

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = _sceneRenderTarget;

            var viewport = _graphics.Viewport;
            _graphics.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, _sceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
