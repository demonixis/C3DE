using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.PostProcessing
{
    public class FilmPass : PostProcessPass
    {
        private Effect _filmEffect;
        private float _noiseIntensity;
        private float _scanlineIntensity;
        private float _scanlineCount;

        public bool GrayScaleEnabled { get; set; }

        public float NoiseIntensity
        {
            get { return _noiseIntensity; }
            set
            {
                _noiseIntensity = MathHelper.Clamp(value, 0.0f, 1.0f);
            }
        }

        public float ScanlineIntensity
        {
            get { return _scanlineIntensity; }
            set
            {
                _scanlineIntensity = MathHelper.Clamp(value, 0.0f, 1.0f);
            }
        }

        public float ScanlineCount
        {
            get { return _scanlineCount; }
            set
            {
                _scanlineCount = MathHelper.Clamp(value, 0.0f, 4096.0f);
            }
        }

        public FilmPass()
            : base()
        {
            GrayScaleEnabled = false;
            _noiseIntensity = 1.0f;
            _scanlineIntensity = 0.5f;
            _scanlineCount = 1024.0f;
        }

        public override void Initialize(ContentManager content)
        {
            _filmEffect = content.Load<Effect>("Shaders/PostProcessing/Film");
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _filmEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _filmEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _filmEffect.Parameters["Time"].SetValue(Time.TotalTime);
            _filmEffect.Parameters["GrayScaleEnabled"].SetValue(GrayScaleEnabled);
            _filmEffect.Parameters["NoiseIntensity"].SetValue(_noiseIntensity);
            _filmEffect.Parameters["ScanlineIntensity"].SetValue(_scanlineIntensity);
            _filmEffect.Parameters["ScanlineCount"].SetValue(_scanlineCount);
            _filmEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
