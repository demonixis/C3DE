using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class FilmFilter : PostProcessPass
    {
        private float _noiseIntensity = 1.0f;
        private float _scanlineIntensity = 0.5f;
        private float _scanlineCount = 1024.0f;
        public bool GrayScaleEnabled { get; set; } = false;

        public float NoiseIntensity
        {
            get => _noiseIntensity;
            set => MathHelper.Clamp(value, 0.0f, 1.0f);
        }

        public float ScanlineIntensity
        {
            get => _scanlineIntensity;
            set => _scanlineIntensity = MathHelper.Clamp(value, 0.0f, 1.0f);
        }

        public float ScanlineCount
        {
            get => _scanlineCount;
            set => _scanlineCount = MathHelper.Clamp(value, 0.0f, 4096.0f);
        }

        public FilmFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            _effect = content.Load<Effect>("Shaders/PostProcessing/Film");
        }

        public override void SetupEffect()
        {
            _effect.Parameters["Time"].SetValue(Time.TotalTime);
            _effect.Parameters["GrayScaleEnabled"].SetValue(GrayScaleEnabled);
            _effect.Parameters["NoiseIntensity"].SetValue(_noiseIntensity);
            _effect.Parameters["ScanlineIntensity"].SetValue(_scanlineIntensity);
            _effect.Parameters["ScanlineCount"].SetValue(_scanlineCount);
        }
    }
}
