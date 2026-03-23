using C3DE.Components;
using C3DE.Graphics;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class ProceduralSkySwitcher : Behaviour
    {
        private Rectangle _boxRect;
        private Texture2D _backgroundTexture;

        public override void Start()
        {
            _boxRect = new Rectangle(10, 10, 320, 300);
            _backgroundTexture = TextureFactory.CreateColor(Color.CornflowerBlue, 1, 1);
        }

        public override void OnGUI(GUI ui)
        {
            var sky = _gameObject.Scene.RenderSettings.Skybox;
            if (sky.Mode != SkyboxMode.Procedural || !sky.ProceduralSettings.Enabled)
                return;

            var settings = sky.ProceduralSettings;
            ui.Box(ref _boxRect, "Procedural Sky");

            var row = _boxRect.Y + 36;

            if (DrawToggle(ui, "Auto Cycle", ref row, settings.AutoCycle))
                settings.AutoCycle = !settings.AutoCycle;

            var timeOfDay = settings.TimeOfDay;
            DrawSlider(ui, "Time Of Day", ref row, ref timeOfDay, 0.0f, 1.0f);
            settings.TimeOfDay = timeOfDay;

            var cycleSpeed = settings.CycleSpeed;
            DrawSlider(ui, "Cycle Speed", ref row, ref cycleSpeed, 0.0f, 0.05f);
            settings.CycleSpeed = cycleSpeed;

            var cloudCoverage = settings.CloudCoverage;
            DrawSlider(ui, "Cloud Coverage", ref row, ref cloudCoverage, 0.0f, 0.95f);
            settings.CloudCoverage = cloudCoverage;

            var cloudSpeed = settings.CloudSpeed;
            DrawSlider(ui, "Cloud Speed", ref row, ref cloudSpeed, 0.0f, 0.05f);
            settings.CloudSpeed = cloudSpeed;

            var starIntensity = settings.StarIntensity;
            DrawSlider(ui, "Star Intensity", ref row, ref starIntensity, 0.0f, 2.0f);
            settings.StarIntensity = starIntensity;

            var sunIntensity = settings.SunIntensity;
            DrawSlider(ui, "Sun Intensity", ref row, ref sunIntensity, 0.0f, 2.0f);
            settings.SunIntensity = sunIntensity;

            var moonIntensity = settings.MoonIntensity;
            DrawSlider(ui, "Moon Intensity", ref row, ref moonIntensity, 0.0f, 1.0f);
            settings.MoonIntensity = moonIntensity;
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

            var sliderRect = new Rectangle(_boxRect.X + 150, y, _boxRect.Width - 166, 24);
            value = MathHelper.Clamp(ui.HorizontalSlider(ref sliderRect, value, min, max), min, max);
            y += 28;
        }
    }
}
