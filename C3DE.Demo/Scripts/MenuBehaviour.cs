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
        public static int ButtonWidth = 200;
        public static int ButtonHeight = 45;

        class DemoWidget
        {
            private Rectangle _rect;
            private string _text;
            private int _index;

            public DemoWidget(string text, int index)
            {
                _text = text;
                _index = index;
                _rect = new Rectangle(0, 0, ButtonWidth, ButtonHeight);
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
            _margin = 15;

            _background = GraphicsHelper.CreateGradiantTexture(Color.LightSteelBlue, Color.Linen, Screen.Width, Screen.Height);
            _backgroundRect = new Rectangle(0, 0, Screen.Width, Screen.Height);

            var titleSize = GUI.Skin.Font.MeasureString("C3DE Demos");
            _titleSize = 3.0f;
            _titleRect = new Vector2(Screen.WidthPerTwo - titleSize.X * _titleSize / 2, titleSize.Y * _titleSize);

            _demos = new DemoWidget[Application.SceneManager.Size - 1];

            for (int i = 0; i < _demos.Length; i++)
                _demos[i] = new DemoWidget(Application.SceneManager[i + 1].Name, i + 1);

            float x = Screen.WidthPerTwo - ButtonWidth / 2;
            float y = Screen.HeightPerTwo - ((ButtonHeight + _margin) * _demos.Length) / 2;

            for (int i = 0; i < _demos.Length; i++)
                _demos[i].SetPosition(x, y + i * (ButtonHeight + _margin));
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
