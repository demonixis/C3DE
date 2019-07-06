using System;

namespace Gwen.RichText
{
	public class FontPart : Part
	{
		private Font m_Font;

		public FontPart(Font font = null)
		{
			m_Font = font;
		}

		public override string[] Split(ref Font font)
		{
			font = m_Font;
			return new string[0];
		}
	}
}
