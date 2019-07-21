using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class FastBloom : PostProcessPass
    {
        private Effect _effect;
        private RenderTarget2D _sceneRenderTarget;
        private QuadRenderer _quadRenderer;

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
            _effect = content.Load<Effect>("Shaders/PostProcessing/FastBloom");
            _sceneRenderTarget = GetRenderTarget();
            _quadRenderer = new QuadRenderer(_graphics);
        }

        protected override void OnVRChanged(VRService service)
        {
            base.OnVRChanged(service);
            _sceneRenderTarget.Dispose();
            _sceneRenderTarget = GetRenderTarget();
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D source)
        {
            _graphics.SetRenderTarget(_sceneRenderTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;

            int divider = resolution == Resolution.Low ? 4 : 2;
            float widthMod = resolution == Resolution.Low ? 0.5f : 1.0f;

            _effect.Parameters["Parameter"].SetValue(new Vector4(blurSize * widthMod, 0.0f, threshold, intensity));
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
                _effect.Parameters["Parameter"].SetValue(new Vector4(blurSize * widthMod + (i * 1.0f), 0.0f, threshold, intensity));

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

            _effect.Parameters["BloomTexture"].SetValue(rt);

            Blit(source, _sceneRenderTarget, 0);

            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.ReleaseAll();

            DrawFullscreenQuad(spriteBatch, source, _sceneRenderTarget, _effect);

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = _sceneRenderTarget;

            _graphics.SetRenderTarget(source);
            DrawFullscreenQuad(spriteBatch, _sceneRenderTarget, _sceneRenderTarget.Width, _sceneRenderTarget.Height, null);
        }

        private void Blit(RenderTarget2D source, RenderTarget2D dest, int pass)
        {
            var textureSamplerTexelSize = new Vector4(1.0f / (float)source.Width, 1.0f / (float)source.Height, source.Width, source.Height);

            _graphics.SetRenderTarget(dest);
            _effect.Parameters["MainTexture"].SetValue(source);
            _effect.Parameters["MainTextureTexelSize"].SetValue(textureSamplerTexelSize);
            _effect.CurrentTechnique.Passes[pass].Apply();
            _quadRenderer.RenderFullscreenQuad();
        }
    }
}
