using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace C3DE.UI
{
    public class GUI
    {
        internal static Effect uiEffect;
        internal static Matrix uiMatrix = Matrix.Identity;
        private SpriteBatch _spriteBatch;
        private bool _loaded;
        private Vector2 _cacheVec2;
        private Rectangle _cacheRect;

        /// <summary>
        /// Enable or disable the UI rendering.
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the Skin used to draw the UI.
        /// </summary>
        public static GUISkin Skin { get; set; }

        /// <summary>
        /// Gets or sets the scaling that must used to render the UI. Default is (1.0f, 1.0f).
        /// </summary>
        public static Vector2 Scale
        {
            get { return new Vector2(uiMatrix[0], uiMatrix[5]); }
            set
            {
                uiMatrix[0] = value.X;
                uiMatrix[5] = value.Y;
            }
        }

        public static Effect Effect
        {
            get { return uiEffect; }
            set { uiEffect = value; }
        }

        public static Matrix Matrix
        {
            get => uiMatrix;
            set { uiMatrix = value; }
        }

        public GUI(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
            _loaded = false;
            _cacheRect = Rectangle.Empty;
            _cacheVec2 = Vector2.Zero;

            if (uiMatrix[0] == 1 && uiMatrix[5] == 1)
                uiMatrix = Matrix.Identity;
        }

        public void LoadContent(ContentManager content)
        {
            if (!_loaded)
            {
                _loaded = true;

                if (Skin == null)
                    Skin = new GUISkin();

                Skin.LoadContent(content);
            }
        }

        private void GetPointerPosition(ref Vector2 position)
        {
            position = Input.Mouse.Position / Scale;

            if (Input.Touch.TouchCount > 0)
                position = Input.Touch.GetPosition() / Scale;
        }

        #region Box Widget

        public void Box(Rectangle rect, string text)
        {
            Box(ref rect, text);
        }

        public void Box(ref Rectangle rect, string text)
        {
            _spriteBatch.Draw(Skin.Box, rect, Color.White);

            _cacheVec2 = Skin.Font.MeasureString(text);
            _cacheVec2.X = (rect.X + rect.Width / 2) - (_cacheVec2.X / 2);
            _cacheVec2.Y = rect.Y + Skin.TextMargin;

            Label(_cacheVec2, text);
        }

        #endregion

        #region Button Widget

        public bool Button(Rectangle rect, string text)
        {
            return Button(ref rect, text);
        }

        public bool Button(ref Rectangle rect, string text, float labelScale = 1.0f)
        {
            var index = 0;

            GetPointerPosition(ref _cacheVec2);

            if (rect.Contains(_cacheVec2))
            {
                index = 1;
                if (Input.Mouse.JustClicked() || Input.Touch.JustPressed())
                    index = 2;
            }

            _spriteBatch.Draw(Skin.Buttons[index], rect, Color.White);

            _cacheVec2 = Skin.Font.MeasureString(text) * labelScale;
            _cacheVec2.X = (rect.X + rect.Width / 2) - (_cacheVec2.X / 2);
            _cacheVec2.Y = (rect.Y + rect.Height / 2) - (_cacheVec2.Y / 2);
            Label(_cacheVec2, text, labelScale, 0.0f);

            return index == 2;
        }

        #endregion

        #region Checkbox Widget

        public bool Checkbox(Rectangle rect, string text, bool isChecked)
        {
            return Checkbox(ref rect, text, isChecked);
        }

        public bool Checkbox(ref Rectangle rect, string text, bool isChecked)
        {
            var index = isChecked ? 2 : 0;

            // Draw the first square
            _cacheRect.X = rect.X;
            _cacheRect.Y = rect.Y;
            _cacheRect.Width = rect.Height;
            _cacheRect.Height = rect.Height;

            _spriteBatch.Draw(Skin.Checkbox[0], _cacheRect, Color.White);

            // Draw the text
            text = WrapText(Skin.Font, text, rect.Width);
            _cacheVec2 = Skin.Font.MeasureString(text);
            _cacheVec2.X = rect.X + rect.Height + Skin.SliderMargin;
            _cacheVec2.Y = rect.Y + rect.Height / 2 - _cacheVec2.Y / 2;

            Label(_cacheVec2, text);

            // If checked, draw the check square
            if (isChecked)
            {
                _cacheRect.X = rect.X + 4;
                _cacheRect.Y = rect.Y + 4;
                _cacheRect.Width = rect.Height - 8;
                _cacheRect.Height = rect.Height - 8;

                _spriteBatch.Draw(Skin.Checkbox[2], _cacheRect, Color.White);
            }

            GetPointerPosition(ref _cacheVec2);

            if (rect.Contains(_cacheVec2))
            {
                index = 1;

                if (Input.Mouse.JustClicked() || Input.Touch.JustPressed())
                    index = isChecked ? 0 : 2;

                _cacheRect.X = rect.X + 4;
                _cacheRect.Y = rect.Y + 4;
                _cacheRect.Width = rect.Height - 8;
                _cacheRect.Height = rect.Height - 8; ;

                if (!isChecked)
                    _spriteBatch.Draw(Skin.Checkbox[index], _cacheRect, Color.White);

                // Restore previous state if not clicked
                if (isChecked && index > 0)
                    index = 2;
            }

            return index == 2;
        }

        #endregion

        #region Horizontal and Vertical Slider Widget

        public float HorizontalSlider(ref Rectangle rect, float value, float leftValue = 0.0f, float rightValue = 1.0f)
        {
            return Slider(ref rect, value, leftValue, rightValue, true);
        }

        public float VerticalSlider(ref Rectangle rect, float value, float leftValue = 0.0f, float rightValue = 1.0f)
        {
            return Slider(ref rect, value, leftValue, rightValue, false);
        }

        private float Slider(ref Rectangle rect, float value, float leftValue, float rightValue, bool horizontal)
        {
            // Transform into the correct coordinates.
            value = value / (rightValue + leftValue);

            // Compute the movable slider position.
            _cacheRect.X = rect.X + 4;
            _cacheRect.Y = rect.Y + 4;
            _cacheRect.Width = horizontal ? (int)(rect.Width * value) - 8 : rect.Width - 8;
            _cacheRect.Height = !horizontal ? (int)(rect.Height * value) - 8 : rect.Height - 8;

            GetPointerPosition(ref _cacheVec2);

            _spriteBatch.Draw(Skin.Sliders[0], rect, Color.White);
            _spriteBatch.Draw(Skin.Sliders[1], _cacheRect, Color.White);

            // Update the position.
            if (rect.Contains(_cacheVec2))
            {
                if (Input.Mouse.Drag() || Input.Touch.Pressed())
                {
                    if (horizontal)
                        value = 1 - (rect.Right - _cacheVec2.X) / rect.Width;
                    else
                        value = 1 - (rect.Bottom - _cacheVec2.Y) / rect.Height;
                }
            }

            return (value * rightValue) + (value * leftValue); // Not good
        }

        #endregion

        #region Label Widget

        public void Label(Vector2 position, string text, float scale = 1.0f, float rotation = 0.0f)
        {
            Label(ref position, text, scale, rotation);
        }

        public void Label(ref Vector2 position, string text, float scale = 1.0f, float rotation = 0.0f)
        {
            _cacheVec2.X = scale;
            _cacheVec2.Y = scale;
            _spriteBatch.DrawString(Skin.Font, text, position, Skin.TextColor, rotation, Vector2.Zero, _cacheVec2, SpriteEffects.None, 1);
        }

        #endregion

        #region Texture Widget

        public void DrawTexture(Rectangle rect, Texture2D texture)
        {
            DrawTexture(ref rect, texture, Color.White);
        }

        public void DrawTexture(ref Rectangle rect, Texture2D texture)
        {
            DrawTexture(ref rect, texture, Color.White);
        }

        public void DrawTexture(ref Rectangle rect, Texture2D texture, Color color)
        {
            _spriteBatch.Draw(texture, rect, color);
        }

        public void DrawTexture(ref Vector2 position, Texture2D texture, Color color)
        {
            _spriteBatch.Draw(texture, position, null, color);
        }

        public void DrawTexture(Vector2 position, Texture2D texture, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            DrawTexture(texture, ref position, sourceRectangle, color, rotation, ref origin, ref scale, effect, depth);
        }

        public void DrawTexture(Texture2D texture, ref Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, ref Vector2 origin, ref Vector2 scale, SpriteEffects effect, float depth)
        {
            _spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effect, depth);
        }

        #endregion

        #region Font utilities

        public Vector2 MeasureString(string text)
        {
            return Skin.Font.MeasureString(text);
        }

        private string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');

            StringBuilder sb = new StringBuilder();

            float lineWidth = 0.0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;
            Vector2 wordSize = Vector2.Zero;

            foreach (string word in words)
            {
                wordSize = spriteFont.MeasureString(word);

                if (lineWidth + wordSize.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += wordSize.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = wordSize.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
