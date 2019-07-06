using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gwen.Loader.MonoGame
{
    public class MonoGameAssetLoader : LoaderBase
	{
		private ContentManager m_ContentManager;
		protected ContentManager ContentManager { get { return m_ContentManager; } }

		public MonoGameAssetLoader(ContentManager contentManager)
		{
			m_ContentManager = contentManager;
		}

		public override Stream GetTextureStream(string name)
		{
			throw new NotSupportedException();
		}

		private void XorCipher(byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
			}
		}

		protected void Decompress(Stream output, byte[] bytes)
		{
			if (keyBytes != null)
				XorCipher(bytes);

			using (var inputStream = new MemoryStream(bytes))
			{
				using (var stream = new GZipStream(inputStream, CompressionMode.Decompress))
				{
					stream.CopyTo(output, 4096);
				}
			}
		}

		public override Stream GetXmlStream(string name)
		{
			if (name.Contains(".xml"))
				name = name.Substring(0, name.Length - 4);

			byte[] bytes = null;
			try
			{
				bytes = ContentManager.Load<byte[]>(name);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine(String.Format("XML file '{0}' not found.", name));
				throw;
			}

			var outputStream = new MemoryStream();

			Decompress(outputStream, bytes);

			outputStream.Seek(0, SeekOrigin.Begin);

			return outputStream;
		}

		public virtual Texture2D LoadTexture(Texture texture)
		{
			string assetName = GetTextureName(texture);

			try
			{
				return m_ContentManager.Load<Texture2D>(assetName);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine(String.Format("Texture '{0}' not found.", assetName));
				throw;
			}
		}

		protected virtual string GetTextureName(Texture texture)
		{
			string name = texture.Name;

			if (name.Contains(".png") || name.Contains(".jpg"))
				name = name.Substring(0, name.Length - 4);

			return name;
		}

		public virtual SpriteFont LoadFont(Font font)
		{
			string assetName = GetFontName(font);

			try
			{
				return m_ContentManager.Load<SpriteFont>(assetName);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine(String.Format("Font '{0}' not found.", assetName));
				throw;
			}
		}

		protected virtual string GetFontName(Font font)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendFormat("{0} {1}", font.FaceName, font.RealSize);

			if (font.Italic && font.Bold)
				sb.Append(" Bold Italic");
			else if (font.Italic)
				sb.Append(" Italic");
			else if (font.Bold)
				sb.Append(" Bold");

			return sb.ToString();
		}

		private byte[] keyBytes = null;
		protected byte[] KeyBytes { get { return keyBytes; } }

		private string key = "";
		public string Key
		{
			get { return key; }
			set
			{
				key = value;
				if (!String.IsNullOrWhiteSpace(key))
				{
					keyBytes = new byte[key.Length / 2];
					for (int i = 0; i < key.Length; i += 2)
					{
						keyBytes[i / 2] = Convert.ToByte(key.Substring(i, 2), 16);
					}
				}
				else
				{
					keyBytes = null;
				}
			}
		}
	}
}
