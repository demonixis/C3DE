using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class SSAO : PostProcessPass
    {
        public int Amount { get; set; } = 1;

        public SSAO(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);   
            _effect = content.Load<Effect>("Shaders/PostProcessing/SSAO");
        }

        public override void SetupEffect()
        {
            _effect.Parameters["Amount"].SetValue(Amount);
            _effect.Parameters["MainTextureTexelSize"].SetValue(_textureSamplerTexelSize);
        }
    }
}
