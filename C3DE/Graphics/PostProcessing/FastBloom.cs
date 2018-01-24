using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class FastBloom : PostProcessPass
    {
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;
        private QuadRenderer m_QuadRenderer;

        public enum Resolution
        {
            Low = 0,
            High = 1,
        }

        public enum BlurType
        {
            Standard = 0,
            Sgx = 1,
        }

        public float threshold = 0.25f;
        public float intensity = 2f;
        public float blurSize = 2.0f;
        public Resolution resolution = Resolution.Low;
        public int blurIterations = 1;
        public BlurType blurType = BlurType.Standard;

        public FastBloom(GraphicsDevice graphics) : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/FastBloom");
            m_SceneRenderTarget = GetRenderTarget();
            m_QuadRenderer = new QuadRenderer(m_GraphicsDevice);
        }

        protected override void OnVRChanged(VRService service)
        {
            base.OnVRChanged(service);
            m_SceneRenderTarget.Dispose();
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D source)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            int divider = resolution == Resolution.Low ? 4 : 2;
            float widthMod = resolution == Resolution.Low ? 0.5f : 1.0f;

            m_Effect.Parameters["Parameter"].SetValue(new Vector4(blurSize * widthMod, 0.0f, threshold, intensity));
            //source.filterMode = FilterMode.Bilinear;

            var rtW = source.Width / divider;
            var rtH = source.Height / divider;

            // downsample
            var rt = RenderTexture.GetTemporary(rtW, rtH);
            //rt.filterMode = FilterMode.Bilinear;
            Blit(source, rt, 1);

            var passOffs = blurType == BlurType.Standard ? 0 : 2;

            for (int i = 0; i < blurIterations; i++)
            {
                m_Effect.Parameters["Parameter"].SetValue(new Vector4(blurSize * widthMod + (i * 1.0f), 0.0f, threshold, intensity));

                // vertical blur
                var rt2 = RenderTexture.GetTemporary(rtW, rtH);
                //rt2.filterMode = FilterMode.Bilinear;
                Blit(rt, rt2, 2 + passOffs);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;

                // horizontal blur
                rt2 = RenderTexture.GetTemporary(rtW, rtH);
                //rt2.filterMode = FilterMode.Bilinear;
                Blit(rt, rt2, 3 + passOffs);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }

            m_Effect.Parameters["BloomTexture"].SetValue(rt);

            Blit(source, m_SceneRenderTarget, 0);

            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.ReleaseAll();

            DrawFullscreenQuad(spriteBatch, source, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            m_GraphicsDevice.SetRenderTarget(source);
            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, m_SceneRenderTarget.Width, m_SceneRenderTarget.Height, null);
        }

        private void Blit(RenderTarget2D source, RenderTarget2D dest, int pass)
        {
            var textureSamplerTexelSize = new Vector4(1.0f / (float)source.Width, 1.0f / (float)source.Height, source.Width, source.Height);

            m_GraphicsDevice.SetRenderTarget(dest);
            m_Effect.Parameters["MainTexture"].SetValue(source);
            m_Effect.Parameters["MainTextureTexelSize"].SetValue(textureSamplerTexelSize);
            m_Effect.CurrentTechnique.Passes[pass].Apply();
            m_QuadRenderer.RenderFullscreenQuad();
        }
    }
}
