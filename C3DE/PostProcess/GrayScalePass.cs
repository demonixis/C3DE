using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcess
{
    public class GrayScalePass : PostProcessPass
    {
        private Effect _greyScaleEffect;

        public override void Initialize(ContentManager content)
        {
            _greyScaleEffect = content.Load<Effect>("FX/PostProcess/GrayScale");
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _greyScaleEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _greyScaleEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _greyScaleEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
