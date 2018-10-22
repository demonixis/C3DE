using System;
using System.IO;
using System.Text;
using static global::Gwen.Platform.Platform;

namespace Gwen.Xml
{
	/// <summary>
	/// Implement this in a class that can be used as a source for XML parser.
	/// </summary>
	public interface IXmlSource
	{
		Stream GetStream();
	}

	/// <summary>
	/// XML source as a string.
	/// </summary>
    public class XmlStringSource : IXmlSource
	{
		public XmlStringSource(string xml, Encoding encoding = null)
		{
			m_xml = xml;
			if (encoding == null)
				m_encoding = new UTF8Encoding();
			else
				m_encoding = encoding;
        }

		public Stream GetStream()
		{
			Stream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream, m_encoding);
			writer.Write(m_xml);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		private string m_xml;
		private Encoding m_encoding;
	}

	/// <summary>
	/// XML source as a file.
	/// </summary>
	public class XmlFileSource : IXmlSource
	{
		public XmlFileSource(string fileName)
		{
			m_fileName = fileName;
        }

		public Stream GetStream()
		{
			return Loader.LoaderBase.Loader.GetXmlStream(m_fileName);
		}

		private string m_fileName;
	}
}
