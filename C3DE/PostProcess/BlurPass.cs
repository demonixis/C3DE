using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.PostProcess
{
    public class BlurPass : PostProcessPass
    {
        public float BlurDistance;

        public BlurPass()
        {
            BlurDistance = 0;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/PostProcess/Blur");
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect);
            effect.Parameters["TargetTexture"].SetValue(renderTarget);
            effect.Parameters["blurDistance"].SetValue(BlurDistance);
            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
