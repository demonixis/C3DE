using C3DE.Components;
using C3DE.Graphics.PostProcessing;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace C3DE.Demo.Scripts
{
    struct Widget
    {
        public Rectangle Rect { get; set; }
        public string Name { get; set; }
        public Rectangle RectExt { get; set; }
    }

    public class PostProcessSwitcher : Behaviour
    {
        private Rectangle _boxRect;
        private Widget[] _widgets;
        private Texture2D _backgroundTexture;
        private List<PostProcessPass> _passes;

        public PostProcessSwitcher()
            : base()
        {
            _passes = new List<PostProcessPass>();
        }

        public override void Start()
        {
            var graphics = Application.GraphicsDevice;

            // Setup PostProcess.
#if !DESKTOP
            var colorGrading = new ColorGrading(graphics);
            AddPass(colorGrading);
            colorGrading.LookUpTable = Application.Content.Load<Texture2D>("Textures/Luts/lut_ver4");
#endif

            var oldBloom = new BloomLegacy(graphics);
            AddPass(oldBloom);
            oldBloom.Settings = new BloomLegacySettings("Profile", 0.15f, 1f, 4.0f, 1.0f, 1f, 1f);

            var newBloom = new Bloom(graphics);
            AddPass(newBloom);
            newBloom.SetPreset(new float[] { 10, 1, 1, 1, 1 }, new float[] { 1, 1, 1, 1, 1 }, 1, 5);

            AddPass(new C64Filter(graphics));
            AddPass(new CGAFilter(graphics));
            AddPass(new ConvolutionFilter(graphics));
            AddPass(new FilmFilter(graphics));
            AddPass(new FXAA(graphics));
            AddPass(new GrayScaleFilter(graphics));
            AddPass(new AverageColorFilter(graphics));
            AddPass(new MotionBlur(graphics));

            var refractionPass = new Refraction(graphics);
            AddPass(refractionPass);
            refractionPass.RefractionTexture = Application.Content.Load<Texture2D>("Textures/hexagrid");
            refractionPass.TextureTiling = new Vector2(0.5f);

            // Setup UI
            var titles = new List<string>();

#if !DESKTOP
            titles.Add("Color Grading");
#endif

            titles.AddRange(new string[]
            {
                "Old Bloom", "New Bloom", "C64 Filter",
                "CGA Filter", "Convolution", "Film",
                "FXAA", "GrayScale", "Average Color",
                "Motion Blur", "Refraction"
            });

            var count = titles.Count;
            _boxRect = new Rectangle(Screen.VirtualWidth - 190, 10, 180, 45 * count);
            _widgets = new Widget[count];

            for (int i = 0; i < count; i++)
            {
                _widgets[i] = new Widget();
                _widgets[i].Name = titles[i];

                if (i == 0)
                    _widgets[i].Rect = new Rectangle(_boxRect.X + 10, _boxRect.Y + 30, _boxRect.Width - 20, 30);
                else
                    _widgets[i].Rect = new Rectangle(_boxRect.X + 10, _widgets[i - 1].Rect.Y + 40, _boxRect.Width - 20, 30);

                _widgets[i].RectExt = new Rectangle(_widgets[i].Rect.X - 1, _widgets[i].Rect.Y - 1, _widgets[i].Rect.Width + 1, _widgets[i].Rect.Height + 1);
            }

            _backgroundTexture = GraphicsHelper.CreateTexture(Color.CornflowerBlue, 1, 1);
        }

        public override void Update()
        {
            if (Input.Keys.JustPressed(Keys.U) || Input.Gamepad.JustPressed(Buttons.Start) || Input.Touch.JustPressed(4))
                GUI.Enabled = !GUI.Enabled;
        }

        public override void OnGUI(GUI ui)
        {
            ui.Box(ref _boxRect, "Effects");

            for (int i = 0, l = _widgets.Length; i < l; i++)
            {
                if (_passes[i].Enabled)
                    ui.DrawTexture(_widgets[i].RectExt, _backgroundTexture);

                if (ui.Button(_widgets[i].Rect, _widgets[i].Name))
                    SetPassActive(i);
            }
        }

        private void AddPass(PostProcessPass pass)
        {
            pass.Enabled = false;
            sceneObject.Scene.Add(pass);
            _passes.Add(pass);
        }

        private void SetPassActive(int index)
        {
            _passes[index].Enabled = !_passes[index].Enabled;
        }
    }
}
