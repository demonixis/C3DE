using System;
using System.Collections.Generic;

namespace Gwen.RichText
{
	public class TextPart : Part
	{
		private string m_Text;
		private Color? m_Color;
		private Font m_Font;

		public string Text { get { return m_Text; } }
		public Color? Color { get { return m_Color; } }
		public Font Font { get { return m_Font; } protected set { m_Font = value; } }

		public TextPart(string text)
		{
			m_Text = text;
			m_Color = null;
		}

		public TextPart(string text, Color color)
		{
			m_Text = text;
			m_Color = color;
		}

		public override string[] Split(ref Font font)
		{
			m_Font = font;

			return StringSplit(m_Text);
		}

		protected string[] StringSplit(string str)
		{
			List<string> strs = new List<string>();
			int len = str.Length;
			int index = 0;
			int i;

			while (index < len)
			{
				i = str.IndexOfAny(m_separator, index);
				if (i == index)
				{
					if (str[i] == ' ')
					{
						strs.Add(" ");
						while (index < len && str[index] == ' ')
							index++;
					}
					else
					{
						strs.Add("\n");
						index++;
						if (index < len && str[index - 1] == '\r' && str[index] == '\n')
							index++;
					}
				}
				else if (i != -1)
				{
					if (str[i] == ' ')
					{
						strs.Add(str.Substring(index, i - index + 1));
						index = i + 1;
					}
					else
					{
						strs.Add(str.Substring(index, i - index));
						index = i;
					}
				}
				else
				{
					strs.Add(str.Substring(index));
					break;
				}
			}

			return strs.ToArray();
		}

		private static readonly char[] m_separator = new char[] { ' ', '\n', '\r' };
	}
}
