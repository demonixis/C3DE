using System;
using System.Collections.Generic;

namespace Gwen.RichText
{
	internal abstract class LineBreaker
	{
		private Renderer.RendererBase m_Renderer;
		private Font m_DefaultFont;

		public Renderer.RendererBase Renderer { get { return m_Renderer; } }
		public Font DefaultFont { get { return m_DefaultFont; } }

		public LineBreaker(Renderer.RendererBase renderer, Font defaultFont)
		{
			m_Renderer = renderer;
			m_DefaultFont = defaultFont;
		}

		public abstract List<TextBlock> LineBreak(Paragraph paragraph, int width);
	}
}
