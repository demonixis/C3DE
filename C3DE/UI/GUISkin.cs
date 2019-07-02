using C3DE.Graphics;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.UI
{
    public class GUISkin
    {
        public Texture2D Box { get; set; }
        public Texture2D Border { get; set; }
        public Texture2D[] Buttons { get; set; }
        public Texture2D[] Checkbox { get; set; }
        public Texture2D[] Sliders { get; set; }
        public SpriteFont Font { get; set; }
        public Color TextColor { get; set; }
        public float TextMargin { get; set; }
        public float CheckboxMargin { get; set; }
        public float SliderMargin { get; set; }
        private string _fontName;

        public GUISkin(string fontName = "")
        {
            _fontName = fontName;

            Buttons = new Texture2D[3];
            Checkbox = new Texture2D[3];
            Sliders = new Texture2D[2];

            CheckboxMargin = 5.0f;
            SliderMargin = 5.0f;
            TextMargin = 6.0f;
            TextColor = Color.White;
        }

        public void LoadContent(ContentManager content)
        {
            Box = TextureFactory.CreateBorder(Color.White, new Color(0.08f, 0.12f, 0.16f, 0.7f), 128, 128, 1);

            Border = TextureFactory.CreateBorder(Color.White, new Color(0.08f, 0.12f, 0.16f, 0.7f), 128, 128, 1);

            Buttons[0] = TextureFactory.CreateBorder(Color.White, new Color(0.08f, 0.12f, 0.16f, 0.7f), 128, 48, 1);
            Buttons[1] = TextureFactory.CreateBorder(Color.WhiteSmoke, new Color(0.16f, 0.19f, 0.23f, 0.7f), 128, 48, 1);
            Buttons[2] =  TextureFactory.CreateBorder(Color.LightGray, new Color(0.19f, 0.23f, 0.27f, 0.7f), 128, 48, 1);

            Checkbox[0] = TextureFactory.CreateBorder(Color.White, new Color(0.08f, 0.12f, 0.16f, 0.7f), 48, 48, 2);
            Checkbox[1] = TextureFactory.CreateColor(Color.DarkGray, 1, 1);                        
            Checkbox[2] = TextureFactory.CreateColor(Color.White, 1, 1);

            Sliders[0] = TextureFactory.CreateColor(new Color(0.08f, 0.12f, 0.16f, 0.7f), 1, 1);
            Sliders[1] = TextureFactory.CreateGradiant(Color.LightBlue, Color.CadetBlue, 16, 16);

            if (!string.IsNullOrEmpty(_fontName))
                Font = content.Load<SpriteFont>(_fontName);
        }
    }
}
