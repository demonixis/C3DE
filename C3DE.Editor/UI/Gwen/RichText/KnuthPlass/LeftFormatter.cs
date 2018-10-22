using System;
using System.Collections.Generic;

namespace Gwen.RichText.KnuthPlass
{
	internal class LeftFormatter : Formatter
	{
		public LeftFormatter(Renderer.RendererBase renderer, Font defaultFont)
			: base(renderer, defaultFont)
		{

		}

		public override List<Node> FormatParagraph(Paragraph paragraph)
		{
			List<Node> nodes = new List<Node>();

			Font font = m_DefaultFont;
			int width, height;

			for (int partIndex = 0; partIndex < paragraph.Parts.Count; partIndex++)
			{
				Part part = paragraph.Parts[partIndex];

				string[] words = part.Split(ref font);
				if (font == null)
					font = m_DefaultFont;

				for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
				{
					string word = words[wordIndex];

					if (word[0] == ' ')
						continue;

					MeasureText(font, word, out width, out height);

					nodes.Add(new BoxNode(width, word, part, height));

					if (wordIndex < (words.Length - 1) || partIndex < (paragraph.Parts.Count - 1))
					{
						nodes.Add(new GlueNode(0, 12, 0));
						nodes.Add(new PenaltyNode(0, 0, 0));
						MeasureText(font, " ", out width, out height);
						nodes.Add(new GlueNode(width, -12, 0));
					}
					else
					{
						nodes.Add(new GlueNode(0, LineBreaker.Infinity, 0));
						nodes.Add(new PenaltyNode(0, -LineBreaker.Infinity, 1));
					}
				}
			}

			return nodes;
		}
	}
}
