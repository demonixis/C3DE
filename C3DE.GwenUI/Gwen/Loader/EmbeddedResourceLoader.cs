using System;
using System.IO;
using System.Reflection;

namespace Gwen.Loader
{
	public class EmbeddedResourceLoader : LoaderBase
	{
		private Assembly m_ResourceAssembly;
		private string m_TextureBaseNamespace;
		private string m_XmlBaseNamespace;

		public EmbeddedResourceLoader(Assembly resourceAssembly = null, string textureNamespace = null, string xmlNamespace = null)
		{
			m_ResourceAssembly = resourceAssembly ?? Assembly.GetCallingAssembly();
			string assemblyName = m_ResourceAssembly.FullName.Split(',')[0];
			m_TextureBaseNamespace = textureNamespace != null ? textureNamespace : assemblyName;
			m_XmlBaseNamespace = xmlNamespace != null ? xmlNamespace : assemblyName;
		}

		public override Stream GetTextureStream(string name)
		{
			name = GetFullName(m_TextureBaseNamespace, name);
			return m_ResourceAssembly.GetManifestResourceStream(name);
		}

		public override Stream GetXmlStream(string name)
		{
			name = GetFullName(m_XmlBaseNamespace, name);
			return m_ResourceAssembly.GetManifestResourceStream(name);
		}

		private string GetFullName(string nameSpace, string name)
		{
			name = name.Replace('/', '.');
			name = name.Replace('\\', '.');
			return nameSpace + "." + name;
		}
	}
}
