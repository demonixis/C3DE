using C3DE.Components;
using C3DE.Graphics;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class UIWidgetDemo : Behaviour
    {
        // Positions/Rectangles of widgets
        private Rectangle _backgroundRect;
        private Rectangle _boxRect;
        private Vector2 _text1Position;
        private Vector2 _text2Position;
        private Rectangle _check1Rect;
        private Rectangle _check2Rect;
        private Rectangle _slider1Rect;
        private Rectangle _slider2Rect;
        private Rectangle _slider3Rect;
        private Vector2 _text3Position;
        private Vector2 _text4Position;
        private Rectangle _btn1Rect;
        private Rectangle _btn2Rect;

        // Value of widgets
        private bool _check1Result;
        private bool _check2Result;
        private float _slider1Value;
        private float _slider2Value;
        private float _slider3Value;
        private string _text1Value;
        private string _text2Value;

        private Texture2D _background;

        public override void Start()
        {
            _check1Result = true;
            _check2Result = false;

            _slider1Value = 20;
            _slider2Value = 0.8f;
            _slider3Value = 0.45f;

            _text1Value = "Welcome to the Graphics User Interface module";
            _text2Value = "This is a label which you can use to display informations.\nIt's multiline you just have to use the '\\n' character.";

            _background = TextureFactory.CreateGradiant(Color.LightSkyBlue, Color.LightSteelBlue, 64, 64);
            _backgroundRect = Screen.VirtualScreenRect;

            var width = 600;
            var height = 390;

            _boxRect = new Rectangle(Screen.VirtualWidthPerTwo - width / 2, Screen.VirtualHeightPerTwo - (height >> 1), width, height);

            var temp1 = GUI.Skin.Font.MeasureString(_text1Value);
            var temp2 = GUI.Skin.Font.MeasureString(_text2Value);

            var margin = _boxRect.X + 50;

            _text1Position = new Vector2(margin, _boxRect.Y + 50);
            _text2Position = new Vector2(margin, _text1Position.Y + temp1.Y + 30);

            _check1Rect = new Rectangle(margin, (int)(_text2Position.Y + temp2.Y + 20), 300, 30);
            _check2Rect = new Rectangle(margin, _check1Rect.Bottom + 10, 300, 30);

            _slider1Rect = new Rectangle(margin, _check2Rect.Bottom + 20, 250, 30);
            _slider2Rect = new Rectangle(margin, _slider1Rect.Bottom + 10, 250, 30);

            _text3Position = new Vector2(_slider1Rect.Right + 10, _slider1Rect.Y);
            _text4Position = new Vector2(_slider2Rect.Right + 10, _slider2Rect.Y);

            _slider3Rect = new Rectangle(_boxRect.Right - 35, _boxRect.Y + (_boxRect.Height >> 1) - (350 >> 1), 30, 350);

            _btn1Rect = new Rectangle(_boxRect.X, _boxRect.Bottom + 10, 150, 45);
            _btn2Rect = new Rectangle(_boxRect.Right - 150, _boxRect.Bottom + 10, 150, 45);
        }

        public override void OnGUI(GUI ui)
        {
            ui.DrawTexture(ref _backgroundRect, _background);
            ui.Box(ref _boxRect, "C3DE UI Module");
            ui.Label(ref _text1Position, _text1Value);
            ui.Label(ref _text2Position, _text2Value);
            
            _check1Result = ui.Checkbox(ref _check1Rect, "Enable this feature?", _check1Result);
            _check2Result = ui.Checkbox(ref _check2Rect, "Enable this other feature?", _check2Result);

            _slider1Value = ui.HorizontalSlider(ref _slider1Rect, _slider1Value, 10, 30);
            _slider2Value = ui.HorizontalSlider(ref _slider2Rect, _slider2Value);

            ui.Label(ref _text3Position, _slider1Value.ToString());
            ui.Label(ref _text4Position, _slider2Value.ToString());

            _slider3Value = ui.VerticalSlider(ref _slider3Rect, _slider3Value);

            if (ui.Button(ref _btn1Rect, "Ok"))
                _text2Value = "You've clicked on the OK button";

            if (ui.Button(ref _btn2Rect, "Cancel"))
                _text2Value = "You've clicked on the Cancel button";
        }
    }
}
