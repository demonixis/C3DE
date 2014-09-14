using C3DE.Components;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class MenuBehaviour : Behaviour
    {
        class DemoWidget
        {
            public const int Width = 150;
            public const int Height = 50;

            private Rectangle _rect;
            private string _text;
            private int _index;

            public DemoWidget(string text, int index)
            {
                _text = text;
                _index = index;
                _rect = new Rectangle(0, 0, Width, Height);
            }

            public void SetPosition(float x, float y)
            {
                _rect.X = (int)x;
                _rect.Y = (int)y;
            }

            public void Draw(GUI gui)
            {
                if (gui.Button(ref _rect, _text))
                    Application.LoadLevel(_index);
            }
        }

        private Rectangle _backgroundRect;
        private Texture2D _background;
        private Vector2 _titleRect;
        private float _titleSize;
        private float _margin;
        private DemoWidget[] _demos;

        public override void Start()
        {
            _margin = 25;

            _background = GraphicsHelper.CreateGradiantTexture(Color.LightSteelBlue, Color.Linen, Screen.Width, Screen.Height);
            _backgroundRect = new Rectangle(0, 0, Screen.Width, Screen.Height);

            var titleSize = GUI.Skin.Font.MeasureString("C3DE Demos");
            _titleSize = 3.0f;
            _titleRect = new Vector2(Screen.WidthPerTwo - titleSize.X * _titleSize / 2, titleSize.Y * _titleSize);

            _demos = new DemoWidget[Application.SceneManager.Size - 2];

            for (var i = 2; i < Application.SceneManager.Size; i++)
                _demos[i - 2] = new DemoWidget(Application.SceneManager[i].Name, i);

            float x = Screen.WidthPerTwo - DemoWidget.Width / 2;
            float y = Screen.HeightPerTwo - ((DemoWidget.Height + _margin) * _demos.Length) / 2;

            for (int i = 0; i < _demos.Length; i++)
                _demos[i].SetPosition(x, y + i * (DemoWidget.Height + _margin));
        }

        public override void Update()
        {
            if (Input.Keys.JustPressed(Keys.Escape) || Input.Gamepad.JustPressed(Buttons.Back))
                Application.Quit();
        }

        public override void OnGUI(GUI gui)
        {
            gui.DrawTexture(ref _backgroundRect, _background);
            gui.Label(ref _titleRect, "C3DE Demos", _titleSize);

            for (int i = 0, l = _demos.Length; i < l; i++)
                _demos[i].Draw(gui);
        }
    }
}
