using C3DE.Graphics;
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
        private Rectangle _boxRect;
        private Widget[] _widgets;
        private Texture2D _backgroundTexture;
        private string _title;

        public event Action<int> SelectionChanged = null;

        public SideMenu(string title, string[] names, int selectedIndex)
        {
            _title = title;

            var count = names.Length;
            _widgets = new Widget[count];

            for (int i = 0; i < count; i++)
            {
                _widgets[i] = new Widget();
                _widgets[i].Name = names[i];
                _widgets[i].Selected = i == selectedIndex;
            }

            _backgroundTexture = TextureFactory.CreateColor(Color.CornflowerBlue, 1, 1);

            SetVertical(false);
        }

        public void SetHorizontal(bool top)
        {
            var offset = 10;
            var itemWidth = 160;
            var itemHeight = 25;
            var width = (itemWidth + 2 * offset) * _widgets.Length;
            var height = 35;

            _boxRect = new Rectangle(Screen.VirtualWidthPerTwo - width / 2, top ? offset : Screen.VirtualHeight - height - offset, width, height);

            for (int i = 0; i < _widgets.Length; i++)
            {
                if (i == 0)
                    _widgets[i].Rect = new Rectangle(_boxRect.X + 10, _boxRect.Y + height / 2 - itemHeight / 2, itemWidth, itemHeight);
                else
                    _widgets[i].Rect = new Rectangle(_widgets[i - 1].Rect.X + 10 + itemWidth, _widgets[i - 1].Rect.Y, itemWidth, itemHeight);

                _widgets[i].RectExt = new Rectangle(_widgets[i].Rect.X - 1, _widgets[i].Rect.Y - 1, _widgets[i].Rect.Width + 1, _widgets[i].Rect.Height + 1);
            }
        }

        public void SetVertical(bool left)
        {
            _boxRect = new Rectangle(left ? 10 : Screen.VirtualWidth - 220, 10, 210, 50 * _widgets.Length);

            for (int i = 0; i < _widgets.Length; i++)
            {
                if (i == 0)
                    _widgets[i].Rect = new Rectangle(_boxRect.X + 10, _boxRect.Y + 30, _boxRect.Width - 20, 30);
                else
                    _widgets[i].Rect = new Rectangle(_boxRect.X + 10, _widgets[i - 1].Rect.Y + 40, _boxRect.Width - 20, 30);

                _widgets[i].RectExt = new Rectangle(_widgets[i].Rect.X - 1, _widgets[i].Rect.Y - 1, _widgets[i].Rect.Width + 1, _widgets[i].Rect.Height + 1);
            }
        }

        public void Draw(GUI ui)
        {
            ui.Box(ref _boxRect, _title);

            for (int i = 0, l = _widgets.Length; i < l; i++)
            {
                if (_widgets[i].Selected)
                    ui.DrawTexture(_widgets[i].RectExt, _backgroundTexture);

                if (ui.Button(_widgets[i].Rect, _widgets[i].Name, null))
                    SetSelected(_widgets[i].Selected ? - 1 : i);
            }
        }

        public void SetSelected(int index)
        {
            for (var i = 0; i < _widgets.Length; i++)
            {
                if (i == index)
                    _widgets[i].Selected = !_widgets[i].Selected;
                else
                    _widgets[i].Selected = false;
            }

            SelectionChanged?.Invoke(index);
        }
    }
}
