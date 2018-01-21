using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts.Editor
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

            for (var i = 0; i < m_Items.Length; i++)
            {
                x += padding;

                if (i > 0)
                    x += size.X + padding;

                size = m_Items[i].GetHeaderSize(GUI.Skin.Font);

                y = (int)(height / 2 - size.Y / 2);

                m_Items[i].Compute(GUI.Skin.Font, x, y, padding);
                m_Items[i].ComputeChildren(GUI.Skin.Font, height, padding, Background);
            }
        }

        public void Update()
        {
            for (var i = 0; i < m_Items.Length; i++)
                m_Items[i].Update();
        }

        public void Draw(GUI ui)
        {
            ui.DrawTexture(m_BarRectangle, Background);

            for (var i = 0; i < m_Items.Length; i++)
                m_Items[i].Draw(ui);
        }
    }
}
