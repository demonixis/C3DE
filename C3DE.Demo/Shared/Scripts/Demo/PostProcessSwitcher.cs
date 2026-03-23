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
        private Texture2D _backgroundTexture;

        public override void Start()
        {
            _boxRect = new Rectangle(Screen.VirtualWidth - 360, 10, 350, 720);
            _backgroundTexture = TextureFactory.CreateColor(Color.CornflowerBlue, 1, 1);
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

            ui.Box(ref _boxRect, "Post Processing");

            var row = _boxRect.Y + 36;
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

            DrawSection(ui, "Bloom / AO", ref row);
            if (DrawToggle(ui, "Bloom", ref row, bloom.Enabled))
                bloom.Enabled = !bloom.Enabled;
            var bloomIntensity = bloom.Intensity;
            DrawSlider(ui, "Bloom Intensity", ref row, ref bloomIntensity, 0.0f, 2.0f);
            bloom.Intensity = bloomIntensity;
            var bloomThreshold = bloom.Threshold;
            DrawSlider(ui, "Bloom Threshold", ref row, ref bloomThreshold, 0.0f, 1.5f);
            bloom.Threshold = bloomThreshold;
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
        }

        private void DrawSection(GUI ui, string title, ref int y)
        {
            var pos = new Vector2(_boxRect.X + 12, y);
            ui.Label(pos, title, Color.White);
            y += 24;
        }

        private bool DrawToggle(GUI ui, string label, ref int y, bool enabled)
        {
            var rect = new Rectangle(_boxRect.X + 12, y, _boxRect.Width - 24, 28);
            if (enabled)
                ui.DrawTexture(new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2), _backgroundTexture);

            var clicked = ui.Button(ref rect, label);
            y += 34;
            return clicked;
        }

        private void DrawSlider(GUI ui, string label, ref int y, ref float value, float min, float max)
        {
            var labelPos = new Vector2(_boxRect.X + 16, y + 4);
            ui.Label(labelPos, $"{label}: {value:0.00}", Color.White);

            var sliderRect = new Rectangle(_boxRect.X + 160, y, _boxRect.Width - 176, 24);
            value = MathHelper.Clamp(ui.HorizontalSlider(ref sliderRect, value, min, max), min, max);
            y += 28;
        }

        private void DrawIntSlider(GUI ui, string label, ref int y, ref int value, int min, int max)
        {
            var floatValue = (float)value;
            DrawSlider(ui, label, ref y, ref floatValue, min, max);
            value = (int)MathHelper.Clamp(floatValue + 0.5f, min, max);
        }
    }
}
