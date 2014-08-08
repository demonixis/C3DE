using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.UI
{
    public abstract class GUIElement
    {
        protected Rectangle _rect;

        public Rectangle Rect
        {
            get { return _rect; }
            protected set { _rect = value; }
        }

        public bool Enabled { get; set; }

        public abstract void LoadContent(ContentManager content);

        public abstract void Draw(SpriteBatch spriteBatch, GUISkin skin);
    }
}
