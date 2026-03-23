using C3DE.Components;
using C3DE.Graphics;
using C3DE.Graphics.PostProcessing;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class PostProcessSwitcher : Behaviour
    {
        private Rectangle _boxRect;
        private Rectangle _contentRect;
        private Texture2D _backgroundTexture;
        private float _scrollOffset;

        public override void Start()
        {
            _boxRect = new Rectangle(Screen.VirtualWidth - 360, 10, 350, 420);
            _backgroundTexture = TextureFactory.CreateColor(Color.CornflowerBlue, 1, 1);
            _scrollOffset = 0.0f;
        }

        public override void Update()
        {
            if (Input.Keys.JustPressed(Keys.U) || Input.Gamepad.JustPressed(Buttons.Start) || Input.Touch.JustPressed(4))
                GUI.Enabled = !GUI.Enabled;
        }

        public override void OnGUI(GUI ui)
        {
            var settings = _gameObject.Scene.RenderSettings.PostProcessing;
            var tonemapping = settings.Tonemapping;
            var colorAdjustments = settings.ColorAdjustments;
            var bloom = settings.Bloom;
            var ambientOcclusion = settings.AmbientOcclusion;
            var sharpen = settings.Sharpen;
            var antiAliasing = settings.AntiAliasing;
            var vignette = settings.Vignette;
            var sunFlare = settings.SunFlare;
            var contentRect = new Rectangle(_boxRect.X + 4, _boxRect.Y + 30, _boxRect.Width - 8, _boxRect.Height - 36);

            ui.Box(ref _boxRect, "Post Processing");
            _contentRect = ui.BeginScrollView(ref contentRect, ref _scrollOffset, MeasureContentHeight());

            var row = _contentRect.Y;
            if (DrawToggle(ui, "Stack Active", ref row, settings.Enabled))
                settings.Enabled = !settings.Enabled;

            DrawSection(ui, "Core", ref row);
            if (DrawToggle(ui, "Tonemapping", ref row, tonemapping.Enabled))
                tonemapping.Enabled = !tonemapping.Enabled;
            var exposure = tonemapping.Exposure;
            DrawSlider(ui, "Exposure", ref row, ref exposure, 0.25f, 2.5f);
            tonemapping.Exposure = exposure;

            if (DrawToggle(ui, "Color Adjust", ref row, colorAdjustments.Enabled))
                colorAdjustments.Enabled = !colorAdjustments.Enabled;
            var contrast = colorAdjustments.Contrast;
            DrawSlider(ui, "Contrast", ref row, ref contrast, 0.5f, 1.6f);
            colorAdjustments.Contrast = contrast;
            var saturation = colorAdjustments.Saturation;
            DrawSlider(ui, "Saturation", ref row, ref saturation, 0.0f, 1.8f);
            colorAdjustments.Saturation = saturation;
            var temperature = colorAdjustments.Temperature;
            DrawSlider(ui, "Temperature", ref row, ref temperature, -0.5f, 0.5f);
            colorAdjustments.Temperature = temperature;
            var tint = colorAdjustments.Tint;
            DrawSlider(ui, "Tint", ref row, ref tint, -0.5f, 0.5f);
            colorAdjustments.Tint = tint;
            DrawColorTriplet(ui, "Lift", ref row, colorAdjustments.Lift, -1.0f, 1.0f, out var lift);
            colorAdjustments.Lift = lift;
            DrawColorTriplet(ui, "Gamma", ref row, colorAdjustments.Gamma, 0.25f, 2.0f, out var gamma);
            colorAdjustments.Gamma = gamma;
            DrawColorTriplet(ui, "Gain", ref row, colorAdjustments.Gain, 0.25f, 2.0f, out var gain);
            colorAdjustments.Gain = gain;

            DrawSection(ui, "Bloom / AO", ref row);
            if (DrawToggle(ui, "Bloom", ref row, bloom.Enabled))
                bloom.Enabled = !bloom.Enabled;
            var bloomIntensity = bloom.Intensity;
            DrawSlider(ui, "Bloom Intensity", ref row, ref bloomIntensity, 0.0f, 2.0f);
            bloom.Intensity = bloomIntensity;
            var bloomThreshold = bloom.Threshold;
            DrawSlider(ui, "Bloom Threshold", ref row, ref bloomThreshold, 0.0f, 1.5f);
            bloom.Threshold = bloomThreshold;
            var bloomSoftKnee = bloom.SoftKnee;
            DrawSlider(ui, "Bloom Knee", ref row, ref bloomSoftKnee, 0.05f, 1.0f);
            bloom.SoftKnee = bloomSoftKnee;
            var bloomBlur = bloom.BlurSize;
            DrawSlider(ui, "Bloom Blur", ref row, ref bloomBlur, 0.5f, 4.0f);
            bloom.BlurSize = bloomBlur;
            var bloomIterations = bloom.BlurIterations;
            DrawIntSlider(ui, "Bloom Iter", ref row, ref bloomIterations, 1, 10);
            bloom.BlurIterations = bloomIterations;

            if (DrawToggle(ui, "SSAO", ref row, ambientOcclusion.Enabled))
                ambientOcclusion.Enabled = !ambientOcclusion.Enabled;
            var aoIntensity = ambientOcclusion.Intensity;
            DrawSlider(ui, "AO Intensity", ref row, ref aoIntensity, 0.0f, 2.0f);
            ambientOcclusion.Intensity = aoIntensity;
            var aoRadius = ambientOcclusion.Radius;
            DrawSlider(ui, "AO Radius", ref row, ref aoRadius, 0.25f, 2.5f);
            ambientOcclusion.Radius = aoRadius;
            var aoBias = ambientOcclusion.Bias;
            DrawSlider(ui, "AO Bias", ref row, ref aoBias, 0.0001f, 0.02f);
            ambientOcclusion.Bias = aoBias;
            var aoBlurSharpness = ambientOcclusion.BlurSharpness;
            DrawSlider(ui, "AO Blur", ref row, ref aoBlurSharpness, 0.5f, 8.0f);
            ambientOcclusion.BlurSharpness = aoBlurSharpness;

            DrawSection(ui, "Finish", ref row);
            if (DrawToggle(ui, "Sharpen", ref row, sharpen.Enabled))
                sharpen.Enabled = !sharpen.Enabled;
            var sharpenIntensity = sharpen.Intensity;
            DrawSlider(ui, "Sharpen Amount", ref row, ref sharpenIntensity, 0.0f, 0.4f);
            sharpen.Intensity = sharpenIntensity;

            if (DrawToggle(ui, "Vignette", ref row, vignette.Enabled))
                vignette.Enabled = !vignette.Enabled;
            var vignetteIntensity = vignette.Intensity;
            DrawSlider(ui, "Vignette Int", ref row, ref vignetteIntensity, 0.0f, 1.0f);
            vignette.Intensity = vignetteIntensity;
            var vignetteSmoothness = vignette.Smoothness;
            DrawSlider(ui, "Vignette Smooth", ref row, ref vignetteSmoothness, 0.1f, 1.0f);
            vignette.Smoothness = vignetteSmoothness;

            if (DrawToggle(ui, "FXAA", ref row, antiAliasing.Enabled))
                antiAliasing.Enabled = !antiAliasing.Enabled;

            if (DrawToggle(ui, "Sun Flare", ref row, sunFlare.Enabled))
                sunFlare.Enabled = !sunFlare.Enabled;
            var flareIntensity = sunFlare.Intensity;
            DrawSlider(ui, "Flare Int", ref row, ref flareIntensity, 0.0f, 2.0f);
            sunFlare.Intensity = flareIntensity;
            var flareSize = sunFlare.Size;
            DrawSlider(ui, "Flare Size", ref row, ref flareSize, 0.02f, 0.4f);
            sunFlare.Size = flareSize;
            var flareGhostIntensity = sunFlare.GhostIntensity;
            DrawSlider(ui, "Flare Ghost", ref row, ref flareGhostIntensity, 0.0f, 1.0f);
            sunFlare.GhostIntensity = flareGhostIntensity;

            DrawSection(ui, "Debug", ref row);
            DrawDebugButtons(ui, ref row, settings);

            ui.EndScrollView();
        }

        private float MeasureContentHeight()
        {
            var height = 0.0f;
            height += 34.0f;
            height += 24.0f;
            height += 34.0f + (13.0f * 28.0f);
            height += 34.0f + (4.0f * 28.0f);
            height += 24.0f;
            height += 34.0f + (5.0f * 28.0f);
            height += 34.0f + (4.0f * 28.0f);
            height += 24.0f;
            height += 34.0f + 28.0f;
            height += 34.0f + (2.0f * 28.0f);
            height += 34.0f;
            height += 34.0f + (3.0f * 28.0f);
            height += 24.0f + 34.0f;
            return height + 8.0f;
        }

        private void DrawSection(GUI ui, string title, ref int y)
        {
            var pos = new Vector2(_contentRect.X + 4, y);
            ui.Label(pos, title, Color.White);
            y += 24;
        }

        private bool DrawToggle(GUI ui, string label, ref int y, bool enabled)
        {
            var rect = new Rectangle(_contentRect.X + 4, y, _contentRect.Width - 8, 28);
            if (enabled)
                ui.DrawTexture(new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2), _backgroundTexture);

            var clicked = ui.Button(ref rect, label);
            y += 34;
            return clicked;
        }

        private void DrawSlider(GUI ui, string label, ref int y, ref float value, float min, float max)
        {
            var labelPos = new Vector2(_contentRect.X + 8, y + 4);
            ui.Label(labelPos, $"{label}: {value:0.00}", Color.White);

            var sliderRect = new Rectangle(_contentRect.X + 152, y, _contentRect.Width - 164, 24);
            value = MathHelper.Clamp(ui.HorizontalSlider(ref sliderRect, value, min, max), min, max);
            y += 28;
        }

        private void DrawIntSlider(GUI ui, string label, ref int y, ref int value, int min, int max)
        {
            var floatValue = (float)value;
            DrawSlider(ui, label, ref y, ref floatValue, min, max);
            value = (int)MathHelper.Clamp(floatValue + 0.5f, min, max);
        }

        private void DrawColorTriplet(GUI ui, string label, ref int y, Vector3 value, float min, float max, out Vector3 output)
        {
            var x = value.X;
            DrawSlider(ui, $"{label} R", ref y, ref x, min, max);
            var yValue = value.Y;
            DrawSlider(ui, $"{label} G", ref y, ref yValue, min, max);
            var z = value.Z;
            DrawSlider(ui, $"{label} B", ref y, ref z, min, max);
            output = new Vector3(x, yValue, z);
        }

        private void DrawDebugButtons(GUI ui, ref int y, PostProcessingSettings settings)
        {
            var buttonWidth = (_contentRect.Width - 20) / 4;
            DrawDebugButton(ui, new Rectangle(_contentRect.X + 4, y, buttonWidth, 28), "Final", PostProcessDebugView.Final, settings);
            DrawDebugButton(ui, new Rectangle(_contentRect.X + 8 + buttonWidth, y, buttonWidth, 28), "Scene", PostProcessDebugView.SceneColor, settings);
            DrawDebugButton(ui, new Rectangle(_contentRect.X + 12 + buttonWidth * 2, y, buttonWidth, 28), "Bloom", PostProcessDebugView.Bloom, settings);
            DrawDebugButton(ui, new Rectangle(_contentRect.X + 16 + buttonWidth * 3, y, buttonWidth, 28), "AO", PostProcessDebugView.AmbientOcclusion, settings);
            y += 34;
        }

        private void DrawDebugButton(GUI ui, Rectangle rect, string label, PostProcessDebugView mode, PostProcessingSettings settings)
        {
            if (settings.DebugView == mode)
                ui.DrawTexture(new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2), _backgroundTexture);

            if (ui.Button(ref rect, label))
                settings.DebugView = mode;
        }
    }
}
