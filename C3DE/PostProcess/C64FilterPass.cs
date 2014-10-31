using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcess
{
    public class C64FilterPass : PostProcessPass
    {
        private Effect _c64filterEffect;

        public override void Initialize(ContentManager content)
        {
            _c64filterEffect = content.Load<Effect>("FX/PostProcess/C64Filter");
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _c64filterEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _c64filterEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _c64filterEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
