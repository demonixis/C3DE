using System;
using System.IO;

namespace Gwen.Loader
{
	public abstract class LoaderBase : ILoader
	{
		private static ILoader m_Loader;
		public static ILoader Loader { get { System.Diagnostics.Debug.Assert(m_Loader != null);  return m_Loader; } }

		public static void Init(ILoader loader)
		{
			m_Loader = loader;
		}

		public abstract Stream GetTextureStream(string name);
		public abstract Stream GetXmlStream(string name);
	}
}
