using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Editor.UI
{
    public class MenuBar
    {
        private MenuItem[] m_Items;
        private Rectangle m_BarRectangle;

        public Texture2D Background { get; set; }

        public MenuBar(MenuItem[] items, Texture2D background)
        {
            m_Items = items;
            Background = background;
        }

        public void Compute(int height, int padding)
        {
            Vector2 size = Vector2.Zero;
            float x = 0;
            float y = 0;

            m_BarRectangle.Width = Screen.Width;
            m_BarRectangle.Height = height;

            var font = Application.Content.Load<SpriteFont>("Font/Menu");

            for (var i = 0; i < m_Items.Length; i++)
            {
                x += padding;

                if (i > 0)
                    x += size.X + padding;

                size = m_Items[i].GetHeaderSize(font);

                y = (int)(height / 2 - size.Y / 2);

                
                m_Items[i].Compute(font, x, y, padding);
                m_Items[i].ComputeChildren(font, height, padding, Background);
            }
        }

        public void Update()
        {
            for (var i = 0; i < m_Items.Length; i++)
                m_Items[i].Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Background, m_BarRectangle, Color.White);

            for (var i = 0; i < m_Items.Length; i++)
                m_Items[i].Draw(spriteBatch);
        }
    }
}
