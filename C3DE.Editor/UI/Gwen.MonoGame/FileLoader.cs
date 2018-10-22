using System;
using System.IO;
using Gwen.Loader;

namespace Gwen.Platform.Windows
{
	public class FileLoader : LoaderBase
	{
		private string m_TextureBasePath;
		private string m_XmlBasePath;

		public FileLoader(string textureBasePath = null, string xmlBasePath = null)
		{
			m_TextureBasePath = textureBasePath != null ? textureBasePath : AppDomain.CurrentDomain.BaseDirectory;
			m_XmlBasePath = xmlBasePath != null ? xmlBasePath : AppDomain.CurrentDomain.BaseDirectory;
		}

		public override Stream GetTextureStream(string name)
		{
			return GetStream(m_TextureBasePath, name);
		}

		public override Stream GetXmlStream(string name)
		{
			return GetStream(m_XmlBasePath, name);
		}

		private Stream GetStream(string basePath, string name)
		{
			if (File.Exists(name))
			{
				return File.Open(name, FileMode.Open, FileAccess.Read);
			}

			name = name.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

			string path = Path.Combine(basePath, name);
			return File.Open(path, FileMode.Open, FileAccess.Read);
		}
	}
}
