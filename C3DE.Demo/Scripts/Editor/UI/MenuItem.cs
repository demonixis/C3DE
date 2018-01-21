using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scripts.Editor
{
    public class MenuItem
    {
        public enum MenuItemState
        {
            None = 0, Visible, Over, Pressed
        }

        private bool m_Show;
        private Texture2D m_ChildTexture;
        private Rectangle m_ChildRectangle;
        public MenuItem Parent { get; private set; }
        public Rectangle Rectangle { get; private set; }
        public string Header { get; private set; }
        public MenuItem[] Children { get; private set; }
        public Vector2 Position { get; set; }
        public MenuItemState State { get; set; }

        public Action<MenuItem> OnClicked = null;

        public MenuItem(string header, Action<MenuItem> callback = null, MenuItem[] children = null)
        {
            Header = header;
            OnClicked = callback;
            Children = children ?? new MenuItem[0];
            for (var i = 0; i < Children.Length; i++)
                children[i].Parent = this;
        }

        public void Close() => m_Show = false;

        public void Compute(SpriteFont font, float x, float y, int padding)
        {
            var size = GetHeaderSize(font);
            Position = new Vector2(x, y);
            Rectangle = new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y);
        }

        public void ComputeChildren(SpriteFont font, int barHeight, int padding, Texture2D texture)
        {
            if (Children.Length == 0)
                return;

            m_ChildTexture = texture;

            Vector2 position = Vector2.Zero;
            Rectangle rectangle = Rectangle.Empty;
            Vector2 size = size = GetChildSize(font, 0);

            position.Y = barHeight;

            for (var i = 0; i < Children.Length; i++)
            {
                position.X = Position.X + padding;
                position.Y += Position.Y;

                if (i > 0)
                    position.Y += padding;

                var sideMenu = false;
                if (Parent != null && Parent.Parent != null)
                    sideMenu = true;

                if (sideMenu)
                {
                    position.X = Position.X + Rectangle.Width;
                    position.Y = Position.Y;
                }

                // Last size
                if (i > 0)
                    position.Y += size.Y;

                size = GetChildSize(font, i);

                rectangle.X = (int)position.X;
                rectangle.Y = (int)position.Y;
                rectangle.Width = (int)size.X;
                rectangle.Height = (int)size.Y;

                Children[i].Position = position;
                Children[i].Rectangle = rectangle;
                m_ChildRectangle.Width = Math.Max(m_ChildRectangle.Width, rectangle.Width + padding * 2);
            }

            m_ChildRectangle.X = (int)position.X - padding;
            m_ChildRectangle.Y = barHeight;
            m_ChildRectangle.Height = (int)((size.Y + padding * 2) * Children.Length);
        }

        public void Update()
        {
            var overlap = Rectangle.Contains(Input.Mouse.Position);

            if (overlap)
            {
                State = MenuItemState.Over;

                if (Input.Mouse.Down(Inputs.MouseButton.Left))
                    State = MenuItemState.Pressed;

                if (Input.Mouse.JustClicked(Inputs.MouseButton.Left))
                {
                    OnClicked?.Invoke(this);
                    m_Show = !m_Show;
                }
            }
            else
                State = Parent == null ? MenuItemState.Visible : MenuItemState.None;

            if (Children.Length == 0 || State == MenuItemState.None)
                return;

            for (var i = 0; i < Children.Length; i++)
                Children[i].Update();
        }

        public void Draw(GUI ui)
        {
            ui.Label(Position, Header, GetColor());

            if (Children.Length == 0 || !m_Show)
                return;

            ui.DrawTexture(m_ChildRectangle, m_ChildTexture);

            for (var i = 0; i < Children.Length; i++)
                ui.Label(Children[i].Position, Children[i].Header, Children[i].GetColor());
        }

        private Color GetColor()
        {
            if (State == MenuItemState.Over)
                return new Color(0.15f, 0.15f, 0.15f);
            else if (State == MenuItemState.Pressed)
                return new Color(0.3f, 0.3f, 0.3f);
            else
                return Color.Black;
        }

        public Vector2 GetHeaderSize(SpriteFont font) => font.MeasureString(Header);
        public Vector2 GetChildSize(SpriteFont font, int index) => Children[index].GetHeaderSize(font);
    }
}
