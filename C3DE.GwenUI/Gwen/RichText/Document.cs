using System;
using System.Collections.Generic;

namespace Gwen.RichText
{
	public class Document
	{
		private List<Paragraph> m_Paragraphs = new List<Paragraph>();

		public List<Paragraph> Paragraphs { get { return m_Paragraphs; } }

		public Document()
		{
		}

		public Document(string text)
		{
			Paragraph paragraph = new Paragraph();
			paragraph.Text(text);
			m_Paragraphs.Add(paragraph);
		}

		public Paragraph Paragraph(Margin margin = new Margin(), int firstIndent = 0, int remainingIndent = 0)
		{
			Paragraph paragraph = new Paragraph(margin, firstIndent, remainingIndent);

			m_Paragraphs.Add(paragraph);

			return paragraph;
		}

		public ImageParagraph Image(string imageName, Size? imageSize = null, Rectangle? textureRect = null, Color? imageColor = null, Margin margin = new Margin(), int indent = 0)
		{
			ImageParagraph paragraph = new ImageParagraph(margin, indent);
			paragraph.Image(imageName, imageSize, textureRect, imageColor);

			m_Paragraphs.Add(paragraph);

			return paragraph;
		}

		public ImageParagraph Image(string imageName, Margin margin, int indent)
		{
			ImageParagraph paragraph = new ImageParagraph(margin, indent);
			paragraph.Image(imageName);

			m_Paragraphs.Add(paragraph);

			return paragraph;
		}

		public ImageParagraph Image(string imageName, Size imageSize, Margin margin = new Margin(), int indent = 0)
		{
			ImageParagraph paragraph = new ImageParagraph(margin, indent);
			paragraph.Image(imageName, imageSize);

			m_Paragraphs.Add(paragraph);
		
			return paragraph;
		}
	}
}
