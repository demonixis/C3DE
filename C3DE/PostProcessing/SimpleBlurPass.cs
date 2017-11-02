using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcessing
{
    public class SimpleBlurPass : PostProcessPass
    {
        private Effect _blurEffect;

        public float BlurDistance;

        public SimpleBlurPass()
             : base()
        {
            BlurDistance = 0;
        }

        public override void Initialize(ContentManager content)
        {
            _blurEffect = content.Load<Effect>("Shaders/PostProcessing/SimpleBlur");
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _blurEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _blurEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _blurEffect.Parameters["BlurDistance"].SetValue(BlurDistance);
            _blurEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
