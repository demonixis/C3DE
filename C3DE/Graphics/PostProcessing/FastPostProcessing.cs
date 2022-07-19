using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class FastPostProcessing : PostProcessPass
    {
        public float Exposure { get; set; } = 1.0f;
        public float BloomSize { get; set; } = 512;
        public float BloomAmount { get; set; } = 0.5f;
        public float BloomPower { get; set; } = 0.5f;

        public FastPostProcessing(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            _effect = content.Load<Effect>("Shaders/PostProcessing/FastPP");
        }

        public override void SetupEffect()
        {
            _effect.Parameters["Exposure"].SetValue(Exposure);
            _effect.Parameters["BloomSize"].SetValue(BloomSize);
            _effect.Parameters["BloomAmount"].SetValue(BloomAmount);
            _effect.Parameters["BloomPower"].SetValue(BloomPower);
            _effect.Parameters["TextureSamplerTexelSize"].SetValue(_textureSamplerTexelSize);
        }
    }
}
