using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.PostProcessing
{
    public class FilmFilter : PostProcessPass
    {
        private Effect m_Effect;
        private float _noiseIntensity = 1.0f;
        private float _scanlineIntensity = 0.5f;
        private float _scanlineCount = 1024.0f;
        private RenderTarget2D m_SceneRenderTarget;

        public bool GrayScaleEnabled { get; set; } = false;

        public float NoiseIntensity
        {
            get { return _noiseIntensity; }
            set { _noiseIntensity = MathHelper.Clamp(value, 0.0f, 1.0f); }
        }

        public float ScanlineIntensity
        {
            get { return _scanlineIntensity; }
            set { _scanlineIntensity = MathHelper.Clamp(value, 0.0f, 1.0f); }
        }

        public float ScanlineCount
        {
            get { return _scanlineCount; }
            set { _scanlineCount = MathHelper.Clamp(value, 0.0f, 4096.0f); }
        }

        public FilmFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        protected override void OnVRChanged(VRService service)
        {
            base.OnVRChanged(service);
            m_SceneRenderTarget.Dispose();
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/Film");
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            m_Effect.Parameters["Time"].SetValue(Time.TotalTime);
            m_Effect.Parameters["GrayScaleEnabled"].SetValue(GrayScaleEnabled);
            m_Effect.Parameters["NoiseIntensity"].SetValue(_noiseIntensity);
            m_Effect.Parameters["ScanlineIntensity"].SetValue(_scanlineIntensity);
            m_Effect.Parameters["ScanlineCount"].SetValue(_scanlineCount);

            DrawFullscreenQuad(spriteBatch, sceneRT, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            m_GraphicsDevice.SetRenderTarget(sceneRT);
            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, m_SceneRenderTarget.Width, m_SceneRenderTarget.Height, null);
        }
    }
}
