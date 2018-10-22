using System;
using Gwen.Control.Internal;
using Gwen.Input;

namespace Gwen.Control
{
	/// <summary>
	/// Image alignment inside the button
	/// </summary>
	[Flags]
	public enum ImageAlign
	{
		Left = 1 << 0,
		Right = 1 << 1,
		Top = 1 << 2,
		Bottom = 1 << 3,
		CenterV = 1 << 4,
		CenterH = 1 << 5,
		Fill = 1 << 6,
		LeftSide = 1 << 7,
		Above = 1 << 8,
		Center = CenterV | CenterH,
	}

	/// <summary>
	/// Button control.
	/// </summary>
	[Xml.XmlControl]
	public class Button : ButtonBase
    {
		private Alignment m_Align;
		private Padding m_TextPadding;
		private Text m_Text;
		private ImageAlign m_ImageAlign;
        private ImagePanel m_Image;

		/// <summary>
		/// Text.
		/// </summary>
		[Xml.XmlProperty]
		public virtual string Text { get { return m_Text.String; } set { EnsureText(); m_Text.String = value; } }

		/// <summary>
		/// Font.
		/// </summary>
		[Xml.XmlProperty]
		public Font Font { get { return m_Text.Font; } set { EnsureText(); m_Text.Font = value; } }

		/// <summary>
		/// Text color.
		/// </summary>
		[Xml.XmlProperty]
		public Color TextColor { get { return m_Text.TextColor; } set { EnsureText(); m_Text.TextColor = value; } }

		/// <summary>
		/// Override text color (used by tooltips).
		/// </summary>
		[Xml.XmlProperty]
		public Color TextColorOverride { get { return m_Text.TextColorOverride; } set { EnsureText(); m_Text.TextColorOverride = value; } }

		/// <summary>
		/// Text padding.
		/// </summary>
		[Xml.XmlProperty]
		public Padding TextPadding { get { return m_TextPadding; } set { if (value == m_TextPadding) return; m_TextPadding = value; Invalidate(); } }

		/// <summary>
		/// Text alignment.
		/// </summary>
		[Xml.XmlProperty]
		public Alignment Alignment { get { return m_Align; } set { if (value == m_Align) return; m_Align = value; Invalidate(); } }

		/// <summary>
		/// Determines how the image is aligned inside the button.
		/// </summary>
		[Xml.XmlProperty]
		public ImageAlign ImageAlign { get { return m_ImageAlign; } set { if (m_ImageAlign == value) return; m_ImageAlign = value; Invalidate(); } }

		/// <summary>
		/// Returns the current image name (or null if no image set) or set a new image.
		/// </summary>
		[Xml.XmlProperty]
		public string ImageName
		{
			get
			{
				if (m_Image != null)
					return m_Image.ImageName;
				else
					return null;
			}
			set
			{
				if (m_Image != null && m_Image.ImageName == value)
					return;

				SetImage(value, m_ImageAlign);
			}
		}

		/// <summary>
		/// Gets or sets the size of the image.
		/// </summary>
		[Xml.XmlProperty]
		public Size ImageSize
		{
			get
			{
				if (m_Image != null)
					return m_Image.ImageSize;
				else
					return Size.Zero;
			}
			set
			{
				if (m_Image == null)
					return;

				m_Image.ImageSize = value;
			}
		}

		/// <summary>
		/// Gets or sets the texture coordinates of the image in pixels.
		/// </summary>
		[Xml.XmlProperty]
		public Rectangle ImageTextureRect
		{
			get
			{
				if (m_Image != null)
					return m_Image.TextureRect;
				else
					return Rectangle.Empty;
			}
			set
			{
				if (m_Image == null)
					return;

				m_Image.TextureRect = value;
			}
		}

		/// <summary>
		/// Gets or sets the color of the image.
		/// </summary>
		[Xml.XmlProperty]
		public Color ImageColor
		{
			get
			{
				if (m_Image != null)
					return m_Image.ImageColor;
				else
					return Color.White;
			}
			set
			{
				if (m_Image == null)
					return;

				m_Image.ImageColor = value;
			}
		}

        /// <summary>
        /// Control constructor.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Button(ControlBase parent)
            : base(parent)
        {
            Alignment = Alignment.Center;
            TextPadding = new Padding(3, 3, 3, 3);
			m_ImageAlign = ImageAlign.LeftSide;
        }

		private void EnsureText()
		{
			if (m_Text == null)
			{
				m_Text = new Text(this);
			}
		}

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            base.Render(skin);

            if (ShouldDrawBackground)
            {
                bool drawDepressed = IsDepressed && IsHovered;
                if (IsToggle)
                    drawDepressed = drawDepressed || ToggleState;

                bool bDrawHovered = IsHovered && ShouldDrawHover;

                skin.DrawButton(this, drawDepressed, bDrawHovered, IsDisabled);
            }
        }

		/// <summary>
		/// Sets the button's image.
		/// </summary>
		/// <param name="textureName">Texture name. Null to remove.</param>
		/// <param name="imageAlign">Determines how the image should be aligned.</param>
		public virtual void SetImage(string textureName, ImageAlign imageAlign = ImageAlign.LeftSide)
        {
            if (String.IsNullOrEmpty(textureName))
            {
                if (m_Image != null)
                    m_Image.Dispose();
                m_Image = null;
                return;
            }

            if (m_Image == null)
            {
                m_Image = new ImagePanel(this);
            }

            m_Image.ImageName = textureName;
			m_Image.MouseInputEnabled = false;
			m_ImageAlign = imageAlign;
			m_Image.SendToBack();

			Invalidate();
		}

		protected override Size OnMeasure(Size availableSize)
		{
			if (m_Image == null)
			{
				Size size = Size.Zero;
				if (m_Text != null)
					size = m_Text.Measure(availableSize);

				size += m_TextPadding + Padding;

				return size;
			}
			else
			{
				Size imageSize = m_Image.Measure(availableSize);
				Size textSize = m_Text != null ? m_Text.Measure(availableSize) + m_TextPadding : Size.Zero;

				Size totalSize;
				switch (m_ImageAlign)
				{
					case ImageAlign.LeftSide:
						totalSize = new Size(textSize.Width + imageSize.Width, Math.Max(imageSize.Height, textSize.Height));
						break;
					case ImageAlign.Above:
						totalSize = new Size(Math.Max(imageSize.Width, textSize.Width), textSize.Height + imageSize.Height);
						break;
					default:
						totalSize = Size.Max(imageSize, textSize);
						break;
				}

				totalSize += Padding;

				return totalSize;
			}
		}

		protected override Size OnArrange(Size finalSize)
		{
			if (m_Image == null)
			{
				if (m_Text != null)
				{
					Size innerSize = finalSize - Padding;
					Size textSize = m_Text.MeasuredSize + m_TextPadding;
					Rectangle rect = new Rectangle(Point.Zero, textSize);

					if ((m_Align & Alignment.CenterH) != 0)
						rect.X = (innerSize.Width - rect.Width) / 2;
					else if ((m_Align & Alignment.Right) != 0)
						rect.X = innerSize.Width - rect.Width;

					if ((m_Align & Alignment.CenterV) != 0)
						rect.Y = (innerSize.Height - rect.Height) / 2;
					else if ((m_Align & Alignment.Bottom) != 0)
						rect.Y = innerSize.Height - rect.Height;

					rect.Offset(m_TextPadding + Padding);

					m_Text.Arrange(rect);
				}
			}
			else
			{
				Size innerSize = finalSize - Padding;

				Size imageSize = m_Image.MeasuredSize;
				Size textSize = m_Text != null ? m_Text.MeasuredSize + m_TextPadding : Size.Zero;

				Rectangle rect;
				switch (m_ImageAlign)
				{
					case ImageAlign.LeftSide:
						rect = new Rectangle(Point.Zero, textSize.Width + imageSize.Width, Math.Max(imageSize.Height, textSize.Height));
						break;
					case ImageAlign.Above:
						rect = new Rectangle(Point.Zero, Math.Max(imageSize.Width, textSize.Width), textSize.Height + imageSize.Height);
						break;
					default:
						rect = new Rectangle(Point.Zero, textSize);
						break;
				}

				if ((m_Align & Alignment.Right) != 0)
					rect.X = innerSize.Width - rect.Width;
				else if ((m_Align & Alignment.CenterH) != 0)
					rect.X = (innerSize.Width - rect.Width) / 2;
				if ((m_Align & Alignment.Bottom) != 0)
					rect.Y = innerSize.Height - rect.Height;
				else if ((m_Align & Alignment.CenterV) != 0)
					rect.Y = (innerSize.Height - rect.Height) / 2;

				Rectangle imageRect = new Rectangle(Point.Zero, imageSize);
				Rectangle textRect = new Rectangle(rect.Location, m_Text != null ? m_Text.MeasuredSize : Size.Zero);

				switch (m_ImageAlign)
				{
					case ImageAlign.LeftSide:
						imageRect.Location = new Point(rect.X, rect.Y + (rect.Height - imageSize.Height) / 2);
						textRect.Location = new Point(rect.X + imageSize.Width, rect.Y + (rect.Height - textSize.Height) / 2);
						break;
					case ImageAlign.Above:
						imageRect.Location = new Point(rect.X + (rect.Width - imageSize.Width) / 2, rect.Y);
						textRect.Location = new Point(rect.X + (rect.Width - textSize.Width) / 2, rect.Y + imageSize.Height);
						break;
					case ImageAlign.Fill:
						imageRect.Size = innerSize;
						break;
					default:
						if ((m_ImageAlign & ImageAlign.Right) != 0)
							imageRect.X = innerSize.Width - imageRect.Width;
						else if ((m_ImageAlign & ImageAlign.CenterH) != 0)
							imageRect.X = (innerSize.Width - imageRect.Width) / 2;
						if ((m_ImageAlign & ImageAlign.Bottom) != 0)
							imageRect.Y = innerSize.Height - imageRect.Height;
						else if ((m_ImageAlign & ImageAlign.CenterV) != 0)
							imageRect.Y = (innerSize.Height - imageRect.Height) / 2;
						break;
				}

				imageRect.Offset(Padding);
				m_Image.Arrange(imageRect);

				if (m_Text != null)
				{
					textRect.Offset(Padding + m_TextPadding);
					m_Text.Arrange(textRect);
				}
			}

			return finalSize;
		}

		/// <summary>
		/// Updates control colors.
		/// </summary>
		public override void UpdateColors()
        {
			if (m_Text == null)
				return;

            if (IsDisabled)
            {
                TextColor = Skin.Colors.Button.Disabled;
                return;
            }

            if (IsDepressed || ToggleState)
            {
                TextColor = Skin.Colors.Button.Down;
                return;
            }

            if (IsHovered)
            {
                TextColor = Skin.Colors.Button.Hover;
                return;
            }

            TextColor = Skin.Colors.Button.Normal;
        }
    }
}
