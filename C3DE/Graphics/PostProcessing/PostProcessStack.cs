using C3DE.Components;
using C3DE.Components.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    /// <summary>
    /// Renderer-owned post-processing orchestrator. It composes dedicated passes
    /// such as bloom and AO, then runs a single final composite pass.
    /// </summary>
    public sealed class PostProcessStack
    {
        private readonly GraphicsDevice _graphics;
        private readonly QuadRenderer _quadRenderer;
        private readonly PostProcessRenderTargetPool _targetPool;
        private readonly PostProcessBloomPass _bloomPass;
        private readonly PostProcessAmbientOcclusionPass _ambientOcclusionPass;

        private Effect _copyEffect;
        private Effect _finalCompositeEffect;
        private Texture2D _whiteTexture;
        private Texture2D _blackTexture;

        public PostProcessStack(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _quadRenderer = new QuadRenderer(graphics);
            _targetPool = new PostProcessRenderTargetPool(graphics);
            _bloomPass = new PostProcessBloomPass(graphics);
            _ambientOcclusionPass = new PostProcessAmbientOcclusionPass(graphics);
        }

        public void Initialize(ContentManager content)
        {
            _copyEffect = content.Load<Effect>("Shaders/Copy");
            _finalCompositeEffect = content.Load<Effect>("Shaders/PostProcessing/PostComposite");
            _bloomPass.Initialize(content);
            _ambientOcclusionPass.Initialize(content);

            _whiteTexture = new Texture2D(_graphics, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });
            _blackTexture = new Texture2D(_graphics, 1, 1);
            _blackTexture.SetData(new[] { Color.Black });
        }

        public bool RequiresDepth(PostProcessingSettings settings)
        {
            return settings.AmbientOcclusion.Enabled || settings.SunFlare.Enabled;
        }

        public bool RequiresNormal(PostProcessingSettings settings)
        {
            return false;
        }

        public void Execute(Scene scene, Camera camera, RenderTarget2D sceneColor, RenderTarget2D depthTexture, RenderTarget2D normalTexture)
        {
            var settings = scene.RenderSettings.PostProcessing;
            if (!settings.HasAnyEffectEnabled)
                return;

            _targetPool.Reset();

            var bloomTexture = _bloomPass.Render(sceneColor, settings.Bloom, _targetPool);
            var aoTexture = _ambientOcclusionPass.Render(depthTexture, settings.AmbientOcclusion, _targetPool, sceneColor.Format);
            var output = _targetPool.Rent(sceneColor.Width, sceneColor.Height, sceneColor.Format);

            RenderFinalComposite(scene, camera, sceneColor, output, depthTexture, bloomTexture, aoTexture);
            Copy(output, sceneColor);

            _targetPool.Release(output);
            if (bloomTexture != null)
                _targetPool.Release(bloomTexture);
            if (aoTexture != null)
                _targetPool.Release(aoTexture);

            ResetGpuState();
        }

        private void RenderFinalComposite(Scene scene, Camera camera, RenderTarget2D source, RenderTarget2D destination, RenderTarget2D depthTexture, RenderTarget2D bloomTexture, RenderTarget2D aoTexture)
        {
            var settings = scene.RenderSettings.PostProcessing;
            var color = settings.ColorAdjustments;
            var tonemapping = settings.Tonemapping;
            var vignette = settings.Vignette;
            var aa = settings.AntiAliasing;
            var sharpen = settings.Sharpen;
            var sun = settings.SunFlare;
            var sunPosition = ResolveSunFlarePosition(scene, camera);

            _graphics.SetRenderTarget(destination);
            _graphics.Clear(Color.Transparent);

            _finalCompositeEffect.Parameters["MainTexture"].SetValue(source);
            _finalCompositeEffect.Parameters["BloomTexture"].SetValue(bloomTexture ?? _blackTexture);
            _finalCompositeEffect.Parameters["AmbientOcclusionTexture"].SetValue(aoTexture ?? _whiteTexture);
            _finalCompositeEffect.Parameters["DepthTexture"].SetValue(depthTexture ?? _whiteTexture);
            _finalCompositeEffect.Parameters["MainTextureTexelSize"].SetValue(new Vector4(
                1.0f / source.Width,
                1.0f / source.Height,
                source.Width,
                source.Height));
            _finalCompositeEffect.Parameters["TonemapParams"].SetValue(new Vector4(
                tonemapping.Exposure,
                settings.Bloom.Enabled ? settings.Bloom.Intensity : 0.0f,
                sun.Enabled ? 1.0f : 0.0f,
                tonemapping.Enabled ? 1.0f : 0.0f));
            _finalCompositeEffect.Parameters["ColorAdjustments"].SetValue(new Vector4(
                color.Contrast,
                color.Saturation,
                color.Temperature,
                color.Enabled ? 1.0f : 0.0f));
            _finalCompositeEffect.Parameters["WhiteBalance"].SetValue(new Vector4(
                color.Temperature,
                color.Tint,
                0.0f,
                0.0f));
            _finalCompositeEffect.Parameters["Lift"].SetValue(new Vector4(color.Lift, 0.0f));
            _finalCompositeEffect.Parameters["Gamma"].SetValue(new Vector4(color.Gamma, 0.0f));
            _finalCompositeEffect.Parameters["Gain"].SetValue(new Vector4(color.Gain, 0.0f));
            _finalCompositeEffect.Parameters["VignetteParams"].SetValue(new Vector4(
                vignette.Enabled ? vignette.Intensity : 0.0f,
                MathHelper.Lerp(8.0f, 1.0f, MathHelper.Clamp(vignette.Smoothness, 0.0f, 1.0f)),
                vignette.Roundness,
                0.0f));
            _finalCompositeEffect.Parameters["VignetteColor"].SetValue(vignette.Color.ToVector4());
            _finalCompositeEffect.Parameters["FxaaParams"].SetValue(new Vector4(
                aa.FxaaReduceMin,
                aa.FxaaReduceMul,
                aa.FxaaSpanMax,
                0.0f));
            _finalCompositeEffect.Parameters["SunFlareParams"].SetValue(new Vector4(
                sun.GhostIntensity,
                sun.Size,
                sun.OcclusionDepthThreshold,
                sun.ScreenFade));
            _finalCompositeEffect.Parameters["SunFlareColor"].SetValue(sun.Tint.ToVector4());
            _finalCompositeEffect.Parameters["SunFlarePosition"].SetValue(sunPosition);
            _finalCompositeEffect.Parameters["EffectToggles"].SetValue(new Vector4(
                sharpen.Enabled ? sharpen.Intensity : 0.0f,
                settings.AmbientOcclusion.Enabled ? 1.0f : 0.0f,
                aa.Enabled ? 1.0f : 0.0f,
                sun.Enabled ? sun.Intensity : 0.0f));
            _finalCompositeEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderFullscreenQuad();
        }

        private void Copy(Texture2D source, RenderTarget2D destination)
        {
            _graphics.SetRenderTarget(destination);
            _copyEffect.Parameters["SourceTexture"].SetValue(source);
            _copyEffect.CurrentTechnique.Passes[0].Apply();
            _quadRenderer.RenderFullscreenQuad();
        }

        private void ResetGpuState()
        {
            _graphics.SetRenderTarget(null);

            for (var i = 0; i < 8; i++)
            {
                _graphics.Textures[i] = null;
            }

            _graphics.BlendState = BlendState.Opaque;
            _graphics.DepthStencilState = DepthStencilState.Default;
            _graphics.RasterizerState = RasterizerState.CullCounterClockwise;
            _graphics.Indices = null;
            _graphics.SetVertexBuffer(null);
        }

        private Vector4 ResolveSunFlarePosition(Scene scene, Camera camera)
        {
            var sunLight = GetActiveSunLight(scene);
            if (sunLight == null)
                return Vector4.Zero;

            var worldPosition = camera.Transform.Position - sunLight.Direction * (camera.Far * 0.95f);
            var projected = _graphics.Viewport.Project(worldPosition, camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
            var isVisible = projected.Z >= 0.0f && projected.Z <= 1.0f &&
                            projected.X >= 0.0f && projected.X <= _graphics.Viewport.Width &&
                            projected.Y >= 0.0f && projected.Y <= _graphics.Viewport.Height;

            if (!isVisible)
                return Vector4.Zero;

            return new Vector4(
                projected.X / _graphics.Viewport.Width,
                projected.Y / _graphics.Viewport.Height,
                1.0f,
                projected.Z);
        }

        private static Light GetActiveSunLight(Scene scene)
        {
            Light fallback = null;

            for (var i = 0; i < scene._lights.Count; i++)
            {
                var light = scene._lights[i];
                if (!light.Enabled || !light.GameObject.Enabled || light.Type != LightType.Directional)
                    continue;

                if (light.IsSun)
                    return light;

                fallback ??= light;
            }

            return fallback;
        }
    }
}
