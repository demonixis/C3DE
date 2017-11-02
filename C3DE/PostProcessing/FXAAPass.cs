using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.PostProcessing
{
    public class FXAAPass : PostProcessPass
    {
        private Effect _fxaaEffect;

        public Vector2 TexelSize { get; set; }

        public FXAAPass()
            : base()
        {
            TexelSize = Vector2.One;
        }

        public override void Initialize(ContentManager content)
        {
            _fxaaEffect = content.Load<Effect>("Shaders/PostProcessing/FXAA");
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _fxaaEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _fxaaEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _fxaaEffect.Parameters["TexelSize"].SetValue(TexelSize);
            _fxaaEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
