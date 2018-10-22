using System;
using System.IO;

namespace Gwen.Loader
{
	public interface ILoader
	{
		Stream GetTextureStream(string name);
		Stream GetXmlStream(string name);
	}
}
