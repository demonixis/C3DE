using C3DE.Components;
using C3DE.Materials;
using C3DE.PostProcess;
using C3DE.Prefabs;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    struct Widget
    {
        public Rectangle Rect { get; set; }
        public string Name { get; set; }
    }

    public class PostProcessSwitcher : Behaviour
    {
        private int _activePassIndex;
        private Rectangle _boxRect;
        private Widget[] _widgets;
        private PostProcessPass[] _passes;
        private SimpleBlurPass _simpleBlurPass;
        private int _passCounter;

        public override void Start()
        {
            _passes = new PostProcessPass[9];
            _passCounter = 0;

            // Setup PostProcess.
            var bloomPass = new BloomPass();
            bloomPass.Settings = new BloomSettings("Côôl", 0.15f, 1f, 4.0f, 1.0f, 1f, 1f);
            AddPass(bloomPass);

            AddPass(new C64FilterPass());

            var cgaPass = new CGAFilterPass();
            cgaPass.SetPalette(cgaPass.Palette2LI);
            AddPass(cgaPass);

            var convolutionPass = new ConvolutionPass();
            AddPass(convolutionPass);

            var filmPass = new FilmPass();
            AddPass(filmPass);

            var fxaaPass = new FXAAPass();
            AddPass(fxaaPass);

            AddPass(new GrayScalePass());

            var refractionPass = new RefractionPass();
            refractionPass.RefractionTexture = Application.Content.Load<Texture2D>("Textures/hexagrid");
            refractionPass.TextureTiling = new Vector2(0.5f);
            AddPass(refractionPass);

            _simpleBlurPass = new SimpleBlurPass();
            AddPass(_simpleBlurPass);

            // Setup UI
            var elementsCount = _passes.Length + 1;
            var titles = new string[] { "None", "Bloom", "C64 Filter", "CGA Filter", "Convolution", "Film", "FXAA", "GrayScale", "Refraction", "Simple Blur" };

            _boxRect = new Rectangle(Screen.VirtualWidth - 190, 10, 180, 45 * (_passes.Length + 1));

            _widgets = new Widget[elementsCount];

            for (int i = 0; i < elementsCount; i++)
            {
                _widgets[i] = new Widget();
                _widgets[i].Name = titles[i];

                if (i == 0)
                    _widgets[i].Rect = new Rectangle(_boxRect.X + 10, _boxRect.Y + 30, _boxRect.Width - 20, 30);
                else
                    _widgets[i].Rect = new Rectangle(_boxRect.X + 10, _widgets[i - 1].Rect.Y + 40, _boxRect.Width - 20, 30);
            }

            _activePassIndex = 0;
        }

        public override void Update()
        {
            if (_simpleBlurPass.Enabled)
            {
                float targetValue = 0.0f;

                if (Input.Mouse.Drag())
                    targetValue = Input.Mouse.Delta.X * Time.DeltaTime * 0.05f;

                if (Input.Touch.Delta().X != 0)
                    targetValue = Input.Touch.Delta().X * Time.DeltaTime * 0.05f;

                _simpleBlurPass.BlurDistance = MathHelper.Lerp(_simpleBlurPass.BlurDistance, targetValue, Time.DeltaTime * 5.0f);
            }

            if (Input.Keys.JustPressed(Keys.U) || Input.Gamepad.JustPressed(Buttons.Start) || Input.Touch.JustPressed(4))
                GUI.Enabled = !GUI.Enabled;
        }

        public override void OnGUI(GUI ui)
        {
            ui.Box(ref _boxRect, "Effects");

            for (int i = 0, l = _widgets.Length; i < l; i++)
            {
                if (ui.Button(_widgets[i].Rect, _widgets[i].Name))
                    SetPassActive(i);
            }
        }

        private void AddPass(PostProcessPass pass)
        {
            pass.Enabled = false;
            sceneObject.Scene.Add(pass);
            _passes[_passCounter++] = pass;
        }

        private void SetPassActive(int index)
        {
            if (index == 0)
                foreach (var pass in _passes)
                    pass.Enabled = false;

            if (index > 0)
                _passes[index - 1].Enabled = true;

            return;

            if (_activePassIndex > 0)
                _passes[_activePassIndex - 1].Enabled = false;

            if (index > 0)
                _passes[index - 1].Enabled = true;

            _activePassIndex = index;
        }
    }
}
