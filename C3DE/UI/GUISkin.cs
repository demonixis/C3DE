using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.UI
{
    public class GUISkin
    {
        public Texture2D Background { get; set; }
        public Texture2D Border { get; set; }
        public Texture2D[] Buttons { get; set; }
        public Texture2D[] Checkbox { get; set; }
        public SpriteFont Font { get; set; }
        private string _fontName;

        public GUISkin(string fontName)
        {
            _fontName = fontName;
        }

        public void LoadContent(ContentManager content)
        {
            Background = GraphicsHelper.CreateTexture(Color.Azure, 1, 1);

            Border = GraphicsHelper.CreateBorderTexture(Color.Black, Color.Azure, 128, 128, 2);

            Buttons = new Texture2D[2] {
                GraphicsHelper.CreateBorderTexture(Color.Black, Color.CadetBlue, 128, 48, 2),
                GraphicsHelper.CreateBorderTexture(Color.Gray, Color.CadetBlue, 128, 48, 2)
            };

            Checkbox = new Texture2D[2] {
                GraphicsHelper.CreateBorderTexture(Color.White, Color.Black, 48, 48, 2),
                GraphicsHelper.CreateTexture(Color.Azure, 1, 1)
            };

            if (!string.IsNullOrEmpty(_fontName))
                Font = content.Load<SpriteFont>(_fontName);
        }
    }
}
