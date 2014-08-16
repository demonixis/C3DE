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
        public SpriteFont Font { get; set; }
        public Color TextColor { get; set; }
        public float Margin { get; set; }
        private string _fontName;

        public GUISkin(string fontName = "")
        {
            _fontName = fontName;
            Buttons = new Texture2D[3];
            Checkbox = new Texture2D[3];

        }

        public void LoadContent(ContentManager content)
        {
            Box = GraphicsHelper.CreateTexture(new Color(0.08f, 0.12f, 0.16f, 0.7f), 1, 1);

            Border = GraphicsHelper.CreateBorderTexture(Color.White, new Color(0.08f, 0.12f, 0.16f, 0.7f), 128, 128, 1);

            Buttons[0] = GraphicsHelper.CreateBorderTexture(Color.White, new Color(0.08f, 0.12f, 0.16f, 0.7f), 128, 48, 1);
            Buttons[1] = GraphicsHelper.CreateBorderTexture(Color.WhiteSmoke, new Color(0.16f, 0.19f, 0.23f, 0.7f), 128, 48, 1);
            Buttons[2] =  GraphicsHelper.CreateBorderTexture(Color.LightGray, new Color(0.19f, 0.23f, 0.27f, 0.7f), 128, 48, 1);

            Checkbox[0] = GraphicsHelper.CreateBorderTexture(Color.White, new Color(0.08f, 0.12f, 0.16f, 0.7f), 48, 48, 2);
            Checkbox[1] = GraphicsHelper.CreateTexture(Color.DarkGray, 1, 1);                        
            Checkbox[2] = GraphicsHelper.CreateTexture(Color.White, 1, 1);                           

            TextColor = Color.White;

            Margin = 5.0f;

            if (!string.IsNullOrEmpty(_fontName))
                Font = content.Load<SpriteFont>(_fontName);
        }
    }
}
