using Microsoft.Xna.Framework;

namespace C3DE.Graphics.PostProcessing
{
    public enum PostProcessBloomResolution
    {
        Half = 0,
        Quarter = 1
    }

    public enum PostProcessDebugView
    {
        Final = 0,
        SceneColor = 1,
        Bloom = 2,
        AmbientOcclusion = 3
    }

    public sealed class TonemappingSettings
    {
        public bool Enabled { get; set; }
        public float Exposure { get; set; } = 1.05f;
    }

    public sealed class ColorAdjustmentsSettings
    {
        public bool Enabled { get; set; }
        public float Contrast { get; set; } = 1.05f;
        public float Saturation { get; set; } = 1.03f;
        public float Temperature { get; set; }
        public float Tint { get; set; }
        public Vector3 Lift { get; set; } = Vector3.Zero;
        public Vector3 Gamma { get; set; } = Vector3.One;
        public Vector3 Gain { get; set; } = Vector3.One;
    }

    public sealed class BloomSettings
    {
        public bool Enabled { get; set; }
        public float Threshold { get; set; } = 0.78f;
        public float SoftKnee { get; set; } = 0.5f;
        public float Intensity { get; set; } = 0.65f;
        public float BlurSize { get; set; } = 1.75f;
        public int BlurIterations { get; set; } = 5;
        public PostProcessBloomResolution Resolution { get; set; } = PostProcessBloomResolution.Half;
    }

    public sealed class AmbientOcclusionSettings
    {
        public bool Enabled { get; set; }
        public float Intensity { get; set; } = 0.85f;
        public float Radius { get; set; } = 1.1f;
        public float Bias { get; set; } = 0.0035f;
        public float BlurSharpness { get; set; } = 4.0f;
    }

    public sealed class SharpenSettings
    {
        public bool Enabled { get; set; }
        public float Intensity { get; set; } = 0.08f;
    }

    public sealed class AntiAliasingSettings
    {
        public bool Enabled { get; set; }
        public float FxaaSpanMax { get; set; } = 8.0f;
        public float FxaaReduceMin { get; set; } = 1.0f / 128.0f;
        public float FxaaReduceMul { get; set; } = 1.0f / 8.0f;
    }

    public sealed class VignetteSettings
    {
        public bool Enabled { get; set; }
        public float Intensity { get; set; } = 0.2f;
        public float Smoothness { get; set; } = 0.55f;
        public float Roundness { get; set; } = 1.0f;
        public Color Color { get; set; } = Color.Black;
    }

    public sealed class SunFlareSettings
    {
        public bool Enabled { get; set; }
        public float Intensity { get; set; } = 0.85f;
        public float Size { get; set; } = 0.16f;
        public float GhostIntensity { get; set; } = 0.4f;
        public float ScreenFade { get; set; } = 0.15f;
        public float OcclusionDepthThreshold { get; set; } = 0.999f;
        public Color Tint { get; set; } = new Color(1.0f, 0.9f, 0.65f);
    }

    /// <summary>
    /// Scene-wide post processing configuration used by the renderer-owned pipeline.
    /// </summary>
    public sealed class PostProcessingSettings
    {
        public bool Enabled { get; set; }
        public PostProcessDebugView DebugView { get; set; } = PostProcessDebugView.Final;
        public TonemappingSettings Tonemapping { get; } = new TonemappingSettings();
        public ColorAdjustmentsSettings ColorAdjustments { get; } = new ColorAdjustmentsSettings();
        public BloomSettings Bloom { get; } = new BloomSettings();
        public AmbientOcclusionSettings AmbientOcclusion { get; } = new AmbientOcclusionSettings();
        public SharpenSettings Sharpen { get; } = new SharpenSettings();
        public AntiAliasingSettings AntiAliasing { get; } = new AntiAliasingSettings();
        public VignetteSettings Vignette { get; } = new VignetteSettings();
        public SunFlareSettings SunFlare { get; } = new SunFlareSettings();

        public bool HasAnyEffectEnabled =>
            Enabled && (
            Tonemapping.Enabled ||
            ColorAdjustments.Enabled ||
            Bloom.Enabled ||
            AmbientOcclusion.Enabled ||
            Sharpen.Enabled ||
            AntiAliasing.Enabled ||
            Vignette.Enabled ||
            SunFlare.Enabled);
    }
}
