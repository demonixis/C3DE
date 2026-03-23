using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    internal sealed class PostProcessAmbientOcclusionPass
    {
        private readonly GraphicsDevice _graphics;
        private readonly QuadRenderer _quadRenderer;
        private Effect _effect;

        public PostProcessAmbientOcclusionPass(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _quadRenderer = new QuadRenderer(graphics);
        }

        public void Initialize(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/PostProcessing/AmbientOcclusion");
        }

        public RenderTarget2D Render(
            RenderTarget2D depthTexture,
            AmbientOcclusionSettings settings,
            PostProcessRenderTargetPool pool,
            SurfaceFormat format)
        {
            if (!settings.Enabled || depthTexture == null)
                return null;

            var rawTarget = pool.Rent(depthTexture.Width, depthTexture.Height, format);
            _graphics.SetRenderTarget(rawTarget);
            _graphics.Clear(Color.White);
            _effect.Parameters["DepthTexture"].SetValue(depthTexture);
            _effect.Parameters["AOTexture"].SetValue(depthTexture);
            _effect.Parameters["MainTextureTexelSize"].SetValue(new Vector4(
                1.0f / depthTexture.Width,
                1.0f / depthTexture.Height,
                depthTexture.Width,
                depthTexture.Height));
            _effect.Parameters["AOParams"].SetValue(new Vector4(
                settings.Intensity,
                settings.Radius,
                settings.Bias,
                settings.BlurSharpness));
            _effect.Parameters["BlurDirection"].SetValue(Vector2.Zero);
            _effect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderFullscreenQuad();

            var blurHorizontal = pool.Rent(depthTexture.Width, depthTexture.Height, format);
            _graphics.SetRenderTarget(blurHorizontal);
            _graphics.Clear(Color.White);
            _effect.Parameters["AOTexture"].SetValue(rawTarget);
            _effect.Parameters["BlurDirection"].SetValue(Vector2.UnitX);
            _effect.CurrentTechnique.Passes[1].Apply();
            _quadRenderer.RenderFullscreenQuad();

            var blurVertical = pool.Rent(depthTexture.Width, depthTexture.Height, format);
            _graphics.SetRenderTarget(blurVertical);
            _graphics.Clear(Color.White);
            _effect.Parameters["AOTexture"].SetValue(blurHorizontal);
            _effect.Parameters["BlurDirection"].SetValue(Vector2.UnitY);
            _effect.CurrentTechnique.Passes[2].Apply();
            _quadRenderer.RenderFullscreenQuad();

            pool.Release(rawTarget);
            pool.Release(blurHorizontal);
            return blurVertical;
        }
    }
}
