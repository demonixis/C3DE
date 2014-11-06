using C3DE.Components;
using C3DE.Materials;
using C3DE.PostProcess;
using C3DE.Prefabs;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        private BloomPass _bloomPass;
        private C64FilterPass _c64Pass;
        private ConvolutionPass _convolutionPass;
        private FXAAPass _fxaaPass;
        private GreyScalePass _greyScalePass;
        private RefractionPass _refractionPass;
        private SimpleBlurPass _simpleBlurPass;

        public override void Start()
        {
            _passes = new PostProcessPass[7];

            // Setup PostProcess.
            _bloomPass = new BloomPass();
            _bloomPass.Settings = new BloomSettings("Côôl", 0.15f, 1f, 4.0f, 1.0f, 1f, 1f);
            AddPass(_bloomPass, 0);

            _c64Pass = new C64FilterPass();
            AddPass(_c64Pass, 1);

            _convolutionPass = new ConvolutionPass();
            AddPass(_convolutionPass, 2);

            _fxaaPass = new FXAAPass();
            AddPass(_fxaaPass, 3);

            _greyScalePass = new GreyScalePass();
            AddPass(_greyScalePass, 4);

            _refractionPass = new RefractionPass();
            _refractionPass.RefractionTexture = Application.Content.Load<Texture2D>("Textures/hexagrid");
            AddPass(_refractionPass, 5);

            _simpleBlurPass = new SimpleBlurPass();
            AddPass(_simpleBlurPass, 6);

            // Setup UI
            _boxRect = new Rectangle(Screen.VirtualWidth - 180, 10, 160, 45 * _passes.Length);

            _widgets = new Widget[_passes.Length];
            _widgets[0] = new Widget() { Name = "Bloom", Rect = new Rectangle(_boxRect.X + 10, _boxRect.Y + 30, _boxRect.Width - 20, 30) };
            _widgets[1] = new Widget() { Name = "C64 Filter", Rect = new Rectangle(_boxRect.X + 10, _widgets[0].Rect.Y + 40, _boxRect.Width - 20, 30) };
            _widgets[2] = new Widget() { Name = "Convolution", Rect = new Rectangle(_boxRect.X + 10, _widgets[1].Rect.Y + 40, _boxRect.Width - 20, 30) };
            _widgets[3] = new Widget() { Name = "FXAA", Rect = new Rectangle(_boxRect.X + 10, _widgets[2].Rect.Y + 40, _boxRect.Width - 20, 30) };
            _widgets[4] = new Widget() { Name = "GreyScale", Rect = new Rectangle(_boxRect.X + 10, _widgets[3].Rect.Y + 40, _boxRect.Width - 20, 30) };
            _widgets[5] = new Widget() { Name = "Refraction", Rect = new Rectangle(_boxRect.X + 10, _widgets[4].Rect.Y + 40, _boxRect.Width - 20, 30) };
            _widgets[6] = new Widget() { Name = "Simple Blur", Rect = new Rectangle(_boxRect.X + 10, _widgets[5].Rect.Y + 40, _boxRect.Width - 20, 30) };

            _activePassIndex = 0;
            _passes[0].Enabled = true;
        }

        public override void Update()
        {
            if (_simpleBlurPass.Enabled && (Input.Mouse.Drag() || Input.Touch.Delta().X != 0))
            {
                _simpleBlurPass.BlurDistance += Input.Mouse.Delta.X * Time.DeltaTime * 0.001f;
                _simpleBlurPass.BlurDistance += Input.Touch.Delta().X * Time.DeltaTime * 0.001f;
            }
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

        private void AddPass(PostProcessPass pass, int index)
        {
            pass.Enabled = false;
            sceneObject.Scene.Add(pass);
            _passes[index] = pass;
        }

        private void SetPassActive(int index)
        {
            _passes[_activePassIndex].Enabled = false;
            _passes[index].Enabled = true;
            _activePassIndex = index;
        }
    }
}
