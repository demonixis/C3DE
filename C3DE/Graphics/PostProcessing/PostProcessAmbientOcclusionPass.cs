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

            var target = pool.Rent(depthTexture.Width, depthTexture.Height, format);
            _graphics.SetRenderTarget(target);
            _graphics.Clear(Color.White);
            _effect.Parameters["DepthTexture"].SetValue(depthTexture);
            _effect.Parameters["MainTextureTexelSize"].SetValue(new Vector4(
                1.0f / depthTexture.Width,
                1.0f / depthTexture.Height,
                depthTexture.Width,
                depthTexture.Height));
            _effect.Parameters["AOParams"].SetValue(new Vector4(settings.Intensity, settings.Radius, settings.Bias, 0.0f));
            _effect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderFullscreenQuad();
            return target;
        }
    }
}
