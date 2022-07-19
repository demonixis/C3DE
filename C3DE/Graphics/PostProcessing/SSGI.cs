using C3DE.Components;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class SSGI : PostProcessPass
    {
        public float IndirectAmount { get; set; } = 50.0f;
        public float NoiseAmount { get; set; } = 0.15f;
        public bool Noise { get; set; } = true;
        public int SampleCount { get; set; } = 64;

        public SSGI(GraphicsDevice graphics)
             : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            _effect = content.Load<Effect>("Shaders/PostProcessing/SSGI");
        }

        public override void SetupEffect()
        {
            _effect.Parameters["MainTexTexelSize"].SetValue(_textureSamplerTexelSize);
            _effect.Parameters["InverseProjectionMatrix"].SetValue(InverseProjectionMatrix);
            _effect.Parameters["SampleCount"].SetValue(SampleCount);
            _effect.Parameters["Noise"].SetValue(Noise ? 1 : 0);
            _effect.Parameters["NoiseAmount"].SetValue(NoiseAmount);
            _effect.Parameters["IndirectAmount"].SetValue(IndirectAmount);
            _effect.Parameters["DepthTexture"].SetValue(GetDepthBuffer());
        }
    }
}
