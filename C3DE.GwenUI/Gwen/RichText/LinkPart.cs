using System;

namespace Gwen.RichText
{
	public class LinkPart : TextPart
	{
		private string m_Link;
		private Color? m_HoverColor = null;
		private Font m_HoverFont = null;

		public string Link { get { return m_Link; } }
		public Color? HoverColor { get { return m_HoverColor; } }
		public Font HoverFont { get { return m_HoverFont; } }

		public LinkPart(string text, string link)
			: base(text)
		{
			m_Link = link;
		}

		public LinkPart(string text, string link, Color color, Color? hoverColor = null, Font hoverFont = null)
			: base(text, color)
		{
			m_Link = link;

			if (hoverColor != null)
				m_HoverColor = hoverColor;
			if (hoverFont != null)
				m_HoverFont = hoverFont;
		}

		public override string[] Split(ref Font font)
		{
			this.Font = font;

			return new string[] { Text.Trim() };
		}
	}
}
