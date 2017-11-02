using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcessing
{
    public class GrayScalePass : PostProcessPass
    {
        private Effect _grayScaleEffect;

        public override void Initialize(ContentManager content)
        {
            _grayScaleEffect = content.Load<Effect>("Shaders/PostProcessing/GrayScale");
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _grayScaleEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _grayScaleEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _grayScaleEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
