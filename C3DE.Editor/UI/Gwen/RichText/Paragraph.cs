using System;
using System.Collections.Generic;

namespace Gwen.RichText
{
	public class Paragraph
	{
		private List<Part> m_Parts = new List<Part>();

		private Margin m_Margin;
		private int m_FirstIndent;
		private int m_RemainigIndent;

		public List<Part> Parts { get { return m_Parts; } }

		public Margin Margin { get { return m_Margin; } }
		public int FirstIndent { get { return m_FirstIndent; } }
		public int RemainigIndent { get { return m_RemainigIndent; } }

		public Paragraph(Margin margin = new Margin(), int firstIndent = 0, int remainingIndent = 0)
		{
			m_Margin = margin;
			m_FirstIndent = firstIndent;
			m_RemainigIndent = remainingIndent;
		}

		public Paragraph Text(string text)
		{
			m_Parts.Add(new TextPart(text));

			return this;
		}

		public Paragraph Text(string text, Color color)
		{
			m_Parts.Add(new TextPart(text, color));

			return this;
		}

		public Paragraph Link(string text, string link, Color? color = null, Color? hoverColor = null, Font hoverFont = null)
		{
			m_Parts.Add(color == null ? new LinkPart(text, link) : new LinkPart(text, link, (Color)color, hoverColor, hoverFont));

			return this;
		}

		public Paragraph Font(Font font = null)
		{
			m_Parts.Add(new FontPart(font));

			return this;
		}

		public Paragraph LineBreak()
		{
			m_Parts.Add(new LineBreakPart());

			return this;
		}
	}
}
