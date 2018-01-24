using System;

namespace Gwen.RichText
{
	public class ImageParagraph : Paragraph
	{
		private string m_ImageName;
		private Size? m_ImageSize;
		private Rectangle? m_TextureRect;
		private Color? m_ImageColor;

		public string ImageName { get { return m_ImageName; } }
		public Size? ImageSize { get { return m_ImageSize; } }
		public Rectangle? TextureRect { get { return m_TextureRect; } }
		public Color? ImageColor { get { return m_ImageColor; } }

		public ImageParagraph(Margin margin = new Margin(), int indent = 0)
			: base(margin, indent, indent)
		{
		}

		public ImageParagraph Image(string imageName, Size? imageSize = null, Rectangle? textureRect = null, Color? imageColor = null)
		{
			m_ImageName = imageName;
			if (imageSize != null)
				m_ImageSize = imageSize;
			if (textureRect != null)
				m_TextureRect = textureRect;
			if (imageColor != null)
				m_ImageColor = imageColor;

			return this;
		}
	}
}

