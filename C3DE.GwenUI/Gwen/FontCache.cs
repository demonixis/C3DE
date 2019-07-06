using System;
using System.Collections.Generic;

namespace Gwen
{
	internal sealed class FontCache : IDisposable
	{
		internal static void CreateCache(Renderer.RendererBase renderer)
		{
			m_Instance = new FontCache(renderer);
		}

		internal static void FreeCache()
		{
			if (m_Instance != null)
				m_Instance.Dispose();
		}

		private FontCache(Renderer.RendererBase renderer)
		{
			m_Renderer = renderer;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
			m_Instance = null;
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var font in m_FontCache)
				{
					font.Value.Dispose();
				}

				m_FontCache.Clear();
			}
		}

		public static Font GetFont(string faceName, int size = 10, FontStyle style = 0)
		{
			return m_Instance.InternalGetFont(faceName, size, style);
		}

		private Font InternalGetFont(string faceName, int size, FontStyle style)
		{
			string id = String.Format("{0};{1};{2}", faceName, size, (int)style);
			Font font;
			if (!m_FontCache.TryGetValue(id, out font))
			{
				font = new Font(m_Renderer, faceName, size);

				if ((style & FontStyle.Bold) != 0)
					font.Bold = true;
				if ((style & FontStyle.Italic) != 0)
					font.Italic = true;
				if ((style & FontStyle.Underline) != 0)
					font.Underline = true;
				if ((style & FontStyle.Strikeout) != 0)
					font.Strikeout = true;

				m_FontCache[id] = font;
			}

			return font;
		}

		private static FontCache m_Instance = null;

		private Renderer.RendererBase m_Renderer;
		private Dictionary<string, Font> m_FontCache = new Dictionary<string, Font>();
	}
}
