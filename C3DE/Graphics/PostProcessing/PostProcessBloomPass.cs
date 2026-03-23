using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    internal sealed class PostProcessBloomPass
    {
        private readonly GraphicsDevice _graphics;
        private readonly QuadRenderer _quadRenderer;
        private Effect _effect;

        public PostProcessBloomPass(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _quadRenderer = new QuadRenderer(graphics);
        }

        public void Initialize(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/PostProcessing/FastBloom");
        }

        public RenderTarget2D Render(RenderTarget2D source, BloomSettings settings, PostProcessRenderTargetPool pool)
        {
            if (!settings.Enabled || settings.Intensity <= 0.0f)
                return null;

            var divider = settings.Resolution == PostProcessBloomResolution.Quarter ? 4 : 2;
            var width = System.Math.Max(1, source.Width / divider);
            var height = System.Math.Max(1, source.Height / divider);
            var prefiltered = pool.Rent(width, height, source.Format);

            var texelSize = new Vector4(1.0f / width, 1.0f / height, width, height);
            _effect.Parameters["Parameter"].SetValue(new Vector4(
                settings.BlurSize,
                MathHelper.Clamp(settings.SoftKnee, 0.0001f, 1.0f),
                settings.Threshold,
                settings.Intensity));

            Blit(source, prefiltered, 1, texelSize);

            var working = prefiltered;
            var iterations = System.Math.Max(1, settings.BlurIterations);
            for (var i = 0; i < iterations; i++)
            {
                var blurSize = settings.BlurSize * (1.0f + i * 0.35f);
                _effect.Parameters["Parameter"].SetValue(new Vector4(
                    blurSize,
                    MathHelper.Clamp(settings.SoftKnee, 0.0001f, 1.0f),
                    settings.Threshold,
                    settings.Intensity));

                var horizontal = pool.Rent(width, height, source.Format);
                Blit(working, horizontal, 5, texelSize);
                if (working != prefiltered)
                    pool.Release(working);

                var vertical = pool.Rent(width, height, source.Format);
                Blit(horizontal, vertical, 4, texelSize);
                pool.Release(horizontal);
                working = vertical;
            }

            if (working != prefiltered)
                pool.Release(prefiltered);

            return working;
        }

        private void Blit(Texture2D source, RenderTarget2D destination, int passIndex, Vector4 texelSize)
        {
            _graphics.SetRenderTarget(destination);
            _graphics.Clear(Color.Transparent);
            _effect.Parameters["MainTexture"].SetValue(source);
            _effect.Parameters["MainTextureTexelSize"].SetValue(texelSize);
            _effect.CurrentTechnique.Passes[passIndex].Apply();
            _quadRenderer.RenderFullscreenQuad();
        }
    }
}
