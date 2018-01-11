using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scripts
{
    public struct Widget
    {
        public Rectangle Rect { get; set; }
        public string Name { get; set; }
        public Rectangle RectExt { get; set; }
        public bool Selected { get; set; }
    }

    public class SideMenu
    {
        private Rectangle m_BoxRect;
        private Widget[] m_Widgets;
        private Texture2D m_BackgroundTexture;
        private string m_Title;

        public event Action<int> SelectionChanged = null;

        public SideMenu(string title, string[] names, int selectedIndex)
        {
            m_Title = title;

            var count = names.Length;
            m_Widgets = new Widget[count];

            for (int i = 0; i < count; i++)
            {
                m_Widgets[i] = new Widget();
                m_Widgets[i].Name = names[i];
                m_Widgets[i].Selected = i == selectedIndex;
            }

            m_BackgroundTexture = GraphicsHelper.CreateTexture(Color.CornflowerBlue, 1, 1);

            SetVertical(false);
        }

        public void SetHorizontal(bool top)
        {
            var offset = 10;
            var itemWidth = 160;
            var itemHeight = 25;
            var width = (itemWidth + 2 * offset) * m_Widgets.Length;
            var height = 35;

            m_BoxRect = new Rectangle(Screen.VirtualWidthPerTwo - width / 2, top ? offset : Screen.VirtualHeight - height - offset, width, height);

            for (int i = 0; i < m_Widgets.Length; i++)
            {
                if (i == 0)
                    m_Widgets[i].Rect = new Rectangle(m_BoxRect.X + 10, m_BoxRect.Y + height / 2 - itemHeight / 2, itemWidth, itemHeight);
                else
                    m_Widgets[i].Rect = new Rectangle(m_Widgets[i - 1].Rect.X + 10 + itemWidth, m_Widgets[i - 1].Rect.Y, itemWidth, itemHeight);

                m_Widgets[i].RectExt = new Rectangle(m_Widgets[i].Rect.X - 1, m_Widgets[i].Rect.Y - 1, m_Widgets[i].Rect.Width + 1, m_Widgets[i].Rect.Height + 1);
            }
        }

        public void SetVertical(bool left)
        {
            m_BoxRect = new Rectangle(left ? 10 : Screen.VirtualWidth - 220, 10, 210, 50 * m_Widgets.Length);

            for (int i = 0; i < m_Widgets.Length; i++)
            {
                if (i == 0)
                    m_Widgets[i].Rect = new Rectangle(m_BoxRect.X + 10, m_BoxRect.Y + 30, m_BoxRect.Width - 20, 30);
                else
                    m_Widgets[i].Rect = new Rectangle(m_BoxRect.X + 10, m_Widgets[i - 1].Rect.Y + 40, m_BoxRect.Width - 20, 30);

                m_Widgets[i].RectExt = new Rectangle(m_Widgets[i].Rect.X - 1, m_Widgets[i].Rect.Y - 1, m_Widgets[i].Rect.Width + 1, m_Widgets[i].Rect.Height + 1);
            }
        }

        public void Draw(GUI ui)
        {
            ui.Box(ref m_BoxRect, m_Title);

            for (int i = 0, l = m_Widgets.Length; i < l; i++)
            {
                if (m_Widgets[i].Selected)
                    ui.DrawTexture(m_Widgets[i].RectExt, m_BackgroundTexture);

                if (ui.Button(m_Widgets[i].Rect, m_Widgets[i].Name))
                    SetSelected(m_Widgets[i].Selected ? - 1 : i);
            }
        }

        public void SetSelected(int index)
        {
            for (var i = 0; i < m_Widgets.Length; i++)
            {
                if (i == index)
                    m_Widgets[i].Selected = !m_Widgets[i].Selected;
                else
                    m_Widgets[i].Selected = false;
            }

            SelectionChanged?.Invoke(index);
        }
    }
}
