using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class FastPostProcessing : PostProcessPass
    {
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;

        public float Exposure { get; set; } = 1.0f;
        public float BloomSize { get; set; } = 512;
        public float BloomAmount { get; set; } = 0.5f;
        public float BloomPower { get; set; } = 0.5f;

        public FastPostProcessing(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/FastPP");
            m_SceneRenderTarget = GetRenderTarget();
        }

        protected override void OnVRChanged(VRService service)
        {
            base.OnVRChanged(service);
            m_SceneRenderTarget.Dispose();
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            _graphics.SetRenderTarget(m_SceneRenderTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;

            var textureSamplerTexelSize = new Vector4(1.0f / (float)renderTarget.Width, 1.0f / (float)renderTarget.Height, renderTarget.Width, renderTarget.Height);

            m_Effect.Parameters["Exposure"].SetValue(Exposure);
            m_Effect.Parameters["BloomSize"].SetValue(BloomSize);
            m_Effect.Parameters["BloomAmount"].SetValue(BloomAmount);
            m_Effect.Parameters["BloomPower"].SetValue(BloomPower);
            m_Effect.Parameters["TextureSamplerTexelSize"].SetValue(textureSamplerTexelSize);

            DrawFullscreenQuad(spriteBatch, renderTarget, m_SceneRenderTarget, m_Effect);

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = m_SceneRenderTarget;

            var viewport = _graphics.Viewport;
            _graphics.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, m_SceneRenderTarget.Width, m_SceneRenderTarget.Height, null);
        }
    }
}
