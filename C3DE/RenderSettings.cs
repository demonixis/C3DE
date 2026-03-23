using C3DE.Graphics;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework;

namespace C3DE
{
    public enum FogMode
    {
        None = 0, Linear, Exp, Exp2
    }

    /// <summary>
    /// Define settings used by the renderer.
    /// </summary>
    public class RenderSettings
    {
        internal Vector3 fogColor;
        internal Vector3 ambientColor;
        internal Vector4 fogData;
        internal Skybox skybox;

        /// <summary>
        /// Gets the scene-wide post processing settings consumed by the renderer pipeline.
        /// </summary>
        public PostProcessingSettings PostProcessing { get; } = new PostProcessingSettings();

        /// <summary>
        /// Gets or sets the global ambient color.
        /// </summary>
        public Color AmbientColor
        {
            get { return new Color(ambientColor); }
            set { ambientColor = value.ToVector3(); }
        }

        /// <summary>
        /// Gets or sets the fog color.
        /// </summary>
        public Color FogColor
        {
            get { return new Color(fogColor); }
            set { fogColor = value.ToVector3(); }
        }

        /// <summary>
        /// Enable or disable the fog.
        /// </summary>
        public bool FogEnabled { get; set; }

        /// <summary>
        /// Gets or sets the type of fog.
        /// </summary>
        public FogMode FogMode
        {
            get { return (FogMode)(int)fogData.X; }
            set { fogData.X = (float)value; }
        }

        /// <summary>
        /// Gets or sets the fog density.
        /// </summary>
        public float FogDensity
        {
            get { return fogData.Y; }
            set { fogData.Y = value; }
        }

        /// <summary>
        /// Gets or sets the fog start.
        /// </summary>
        public float FogStart
        {
            get { return fogData.Z; }
            set { fogData.Z = value; }
        }

        /// <summary>
        /// Gets or sets the fog end.
        /// </summary>
        public float FogEnd
        {
            get { return fogData.W; }
            set { fogData.W = value; }
        }

        public Skybox Skybox
        {
            get { return skybox; }
            set { skybox = value; }
        }

        /// <summary>
        /// Create the render setting with default values.
        /// </summary>
        public RenderSettings()
        {
            ambientColor = new Vector3(0.1f, 0.1f, 0.1f);
            fogData = Vector4.Zero;
            FogEnabled = false;
            FogMode = FogMode.None;
            FogDensity = 0.01f;
            FogStart = 20.0f;
            FogEnd = 150.0f;
            fogColor = Vector3.One;
            skybox = new Skybox();
        }

        public void Set(RenderSettings settings)
        {
            ambientColor = settings.ambientColor;
            fogData = settings.fogData;
            fogColor = settings.fogColor;
            FogEnabled = settings.FogEnabled;
            skybox = settings.skybox;

            PostProcessing.Tonemapping.Enabled = settings.PostProcessing.Tonemapping.Enabled;
            PostProcessing.Tonemapping.Exposure = settings.PostProcessing.Tonemapping.Exposure;
            PostProcessing.Enabled = settings.PostProcessing.Enabled;
            PostProcessing.DebugView = settings.PostProcessing.DebugView;
            PostProcessing.ColorAdjustments.Enabled = settings.PostProcessing.ColorAdjustments.Enabled;
            PostProcessing.ColorAdjustments.Contrast = settings.PostProcessing.ColorAdjustments.Contrast;
            PostProcessing.ColorAdjustments.Saturation = settings.PostProcessing.ColorAdjustments.Saturation;
            PostProcessing.ColorAdjustments.Temperature = settings.PostProcessing.ColorAdjustments.Temperature;
            PostProcessing.ColorAdjustments.Tint = settings.PostProcessing.ColorAdjustments.Tint;
            PostProcessing.ColorAdjustments.Lift = settings.PostProcessing.ColorAdjustments.Lift;
            PostProcessing.ColorAdjustments.Gamma = settings.PostProcessing.ColorAdjustments.Gamma;
            PostProcessing.ColorAdjustments.Gain = settings.PostProcessing.ColorAdjustments.Gain;
            PostProcessing.Bloom.Enabled = settings.PostProcessing.Bloom.Enabled;
            PostProcessing.Bloom.Threshold = settings.PostProcessing.Bloom.Threshold;
            PostProcessing.Bloom.SoftKnee = settings.PostProcessing.Bloom.SoftKnee;
            PostProcessing.Bloom.Intensity = settings.PostProcessing.Bloom.Intensity;
            PostProcessing.Bloom.BlurSize = settings.PostProcessing.Bloom.BlurSize;
            PostProcessing.Bloom.BlurIterations = settings.PostProcessing.Bloom.BlurIterations;
            PostProcessing.Bloom.Resolution = settings.PostProcessing.Bloom.Resolution;
            PostProcessing.AmbientOcclusion.Enabled = settings.PostProcessing.AmbientOcclusion.Enabled;
            PostProcessing.AmbientOcclusion.Intensity = settings.PostProcessing.AmbientOcclusion.Intensity;
            PostProcessing.AmbientOcclusion.Radius = settings.PostProcessing.AmbientOcclusion.Radius;
            PostProcessing.AmbientOcclusion.Bias = settings.PostProcessing.AmbientOcclusion.Bias;
            PostProcessing.AmbientOcclusion.BlurSharpness = settings.PostProcessing.AmbientOcclusion.BlurSharpness;
            PostProcessing.Sharpen.Enabled = settings.PostProcessing.Sharpen.Enabled;
            PostProcessing.Sharpen.Intensity = settings.PostProcessing.Sharpen.Intensity;
            PostProcessing.AntiAliasing.Enabled = settings.PostProcessing.AntiAliasing.Enabled;
            PostProcessing.AntiAliasing.FxaaSpanMax = settings.PostProcessing.AntiAliasing.FxaaSpanMax;
            PostProcessing.AntiAliasing.FxaaReduceMin = settings.PostProcessing.AntiAliasing.FxaaReduceMin;
            PostProcessing.AntiAliasing.FxaaReduceMul = settings.PostProcessing.AntiAliasing.FxaaReduceMul;
            PostProcessing.Vignette.Enabled = settings.PostProcessing.Vignette.Enabled;
            PostProcessing.Vignette.Intensity = settings.PostProcessing.Vignette.Intensity;
            PostProcessing.Vignette.Smoothness = settings.PostProcessing.Vignette.Smoothness;
            PostProcessing.Vignette.Roundness = settings.PostProcessing.Vignette.Roundness;
            PostProcessing.Vignette.Color = settings.PostProcessing.Vignette.Color;
            PostProcessing.SunFlare.Enabled = settings.PostProcessing.SunFlare.Enabled;
            PostProcessing.SunFlare.Intensity = settings.PostProcessing.SunFlare.Intensity;
            PostProcessing.SunFlare.Size = settings.PostProcessing.SunFlare.Size;
            PostProcessing.SunFlare.GhostIntensity = settings.PostProcessing.SunFlare.GhostIntensity;
            PostProcessing.SunFlare.ScreenFade = settings.PostProcessing.SunFlare.ScreenFade;
            PostProcessing.SunFlare.OcclusionDepthThreshold = settings.PostProcessing.SunFlare.OcclusionDepthThreshold;
            PostProcessing.SunFlare.Tint = settings.PostProcessing.SunFlare.Tint;
        }
    }
}
