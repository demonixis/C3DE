using System;
using System.Collections.Generic;
using System.IO;

#if WINDOWS
using System.Threading;
using System.Windows.Forms;
#endif

#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
using System.Linq;
#endif

namespace Gwen.Platform.MonoGame
{
    public class MonoGamePlatform : IPlatform
	{
		private DateTime m_FirstTime = DateTime.Now;

		/// <summary>
		/// Gets text from clipboard.
		/// </summary>
		/// <returns>Clipboard text.</returns>
		public string GetClipboardText()
		{
#if WINDOWS
			// code from http://forums.getpaint.net/index.php?/topic/13712-trouble-accessing-the-clipboard/page__view__findpost__p__226140
			string ret = String.Empty;
			Thread staThread = new Thread(
				() =>
				{
					try
					{
						if (!Clipboard.ContainsText())
							return;
						ret = Clipboard.GetText();
					}
					catch (Exception)
					{
						return;
					}
				});
			staThread.SetApartmentState(ApartmentState.STA);
			staThread.Start();
			staThread.Join();
			// at this point either you have clipboard data or an exception
			return ret;
#else
			return null;
#endif
		}

		/// <summary>
		/// Sets the clipboard text.
		/// </summary>
		/// <param name="text">Text to set.</param>
		/// <returns>True if succeeded.</returns>
		public bool SetClipboardText(string text)
		{
#if WINDOWS
			bool ret = false;
			Thread staThread = new Thread(
				() =>
				{
					try
					{
						Clipboard.SetText(text);
						ret = true;
					}
					catch (Exception)
					{
						return;
					}
				});
			staThread.SetApartmentState(ApartmentState.STA);
			staThread.Start();
			staThread.Join();
			// at this point either you have clipboard data or an exception
			return ret;
#else
			return false;
#endif
		}

		/// <summary>
		/// Gets elapsed time since this class was initalized.
		/// </summary>
		/// <returns>Time interval in seconds.</returns>
		public float GetTimeInSeconds()
		{
			return (float)((DateTime.Now - m_FirstTime).TotalSeconds);
		}

		/// <summary>
		/// Changes the mouse cursor.
		/// </summary>
		/// <param name="cursor">Cursor type.</param>
		public void SetCursor(Cursor cursor)
		{
#if WINDOWS
			System.Windows.Forms.Cursor.Current = m_CursorMap[cursor];
#endif
		}

		/// <summary>
		/// Get special folders of the system.
		/// </summary>
		/// <returns>List of folders.</returns>
		public IEnumerable<ISpecialFolder> GetSpecialFolders()
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			List<SpecialFolder> folders = new List<SpecialFolder>();

			try
			{
				folders.Add(new SpecialFolder("Documents", "Libraries", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
				folders.Add(new SpecialFolder("Music", "Libraries", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)));
				folders.Add(new SpecialFolder("Pictures", "Libraries", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)));
				folders.Add(new SpecialFolder("Videos", "Libraries", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)));
			}
			catch (Exception)
			{

			}

			DriveInfo[] drives = null;
			try
			{
				drives = DriveInfo.GetDrives();
			}
			catch (Exception) { }

			if (drives != null)
			{
				foreach (DriveInfo driveInfo in drives)
				{
					try
					{
						if (driveInfo.IsReady)
						{
							if (String.IsNullOrWhiteSpace(driveInfo.VolumeLabel))
								folders.Add(new SpecialFolder(driveInfo.Name, "Computer", driveInfo.Name));
							else
								folders.Add(new SpecialFolder(String.Format("{0} ({1})", driveInfo.VolumeLabel, driveInfo.Name), "Computer", driveInfo.Name));
						}
					}
					catch (Exception) { }
				}
			}

			return folders;
#else
			return new List<ISpecialFolder>();
#endif
		}

		public string GetFileName(string path)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			return Path.GetFileName(path);
#else
			return path;
#endif
		}

		public string GetDirectoryName(string path)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			return Path.GetDirectoryName(path);
#else
			return path;
#endif
		}

		public bool FileExists(string path)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			return File.Exists(path);
#else
			return false;
#endif
		}

		public bool DirectoryExists(string path)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			return Directory.Exists(path);
#else
			return false;
#endif
		}

		public void CreateDirectory(string path)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			Directory.CreateDirectory(path);
#endif
		}

		public string Combine(string path1, string path2)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			return Path.Combine(path1, path2);
#else
			return path1 + '/' + path2;
#endif
		}

		public string Combine(string path1, string path2, string path3)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			return Path.Combine(path1, path2, path3);
#else
			return path1 + '/' + path2 + '/' + path3;
#endif
		}

		public string Combine(string path1, string path2, string path3, string path4)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			return Path.Combine(path1, path2, path3, path4);
#else
			return path1 + '/' + path2 + '/' + path3 + '/' + path4;
#endif
		}

		public string CurrentDirectory
		{
			get
			{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
				return Environment.CurrentDirectory;
#else
				return String.Empty;
#endif
			}
		}

		public IEnumerable<IFileSystemDirectoryInfo> GetDirectories(string path)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			DirectoryInfo di = new DirectoryInfo(path);
			return di.GetDirectories().Select(d => new FileSystemDirectoryInfo(d.FullName, d.LastWriteTime) as IFileSystemDirectoryInfo);
#else
			return new List<IFileSystemDirectoryInfo>();
#endif
		}

		public IEnumerable<IFileSystemFileInfo> GetFiles(string path, string filter)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			DirectoryInfo di = new DirectoryInfo(path);
			return di.GetFiles(filter).Select(f => new FileSystemFileInfo(f.FullName, f.LastWriteTime, f.Length) as IFileSystemFileInfo);
#else
			return new List<IFileSystemFileInfo>();
#endif
		}

		public Stream GetFileStream(string path, bool isWritable)
		{
#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
			return new FileStream(path, isWritable ? FileMode.Create : FileMode.Open, isWritable ? FileAccess.Write : FileAccess.Read);
#else
			return null;
#endif
		}

#if WINDOWS
		private static readonly Dictionary<Cursor, System.Windows.Forms.Cursor> m_CursorMap = new Dictionary<Cursor, System.Windows.Forms.Cursor>
		{
			{ Cursor.Normal, System.Windows.Forms.Cursors.Arrow },
			{ Cursor.Beam, System.Windows.Forms.Cursors.IBeam },
			{ Cursor.SizeNS, System.Windows.Forms.Cursors.SizeNS },
			{ Cursor.SizeWE, System.Windows.Forms.Cursors.SizeWE },
			{ Cursor.SizeNWSE, System.Windows.Forms.Cursors.SizeNWSE },
			{ Cursor.SizeNESW, System.Windows.Forms.Cursors.SizeNESW },
			{ Cursor.SizeAll, System.Windows.Forms.Cursors.SizeAll },
			{ Cursor.No, System.Windows.Forms.Cursors.No },
			{ Cursor.Wait, System.Windows.Forms.Cursors.WaitCursor },
			{ Cursor.Finger, System.Windows.Forms.Cursors.Hand }
		};
#endif

#if WINDOWS || LINUX || MONOMAC || ANDROID || IOS
		private class SpecialFolder : ISpecialFolder
		{
			public SpecialFolder(string name, string category, string path)
			{
				this.Name = name;
				this.Category = category;
				this.Path = path;
			}

			public string Name { get; internal set; }
			public string Category { get; internal set; }
			public string Path { get; internal set; }
		}

		public class FileSystemItemInfo : IFileSystemItemInfo
		{
			public FileSystemItemInfo(string path, DateTime lastWriteTime)
			{
				this.Name = Path.GetFileName(path);
				this.FullName = path;
				this.FormattedLastWriteTime = String.Format("{0} {1}", lastWriteTime.ToShortDateString(), lastWriteTime.ToLongTimeString());
			}

			public string Name { get; internal set; }
			public string FullName { get; internal set; }
			public string FormattedLastWriteTime { get; internal set; }
		}

		public class FileSystemDirectoryInfo : FileSystemItemInfo, IFileSystemDirectoryInfo
		{
			public FileSystemDirectoryInfo(string path, DateTime lastWriteTime)
				: base(path, lastWriteTime)
			{

			}
		}

		public class FileSystemFileInfo : FileSystemItemInfo, IFileSystemFileInfo
		{
			public FileSystemFileInfo(string path, DateTime lastWriteTime, long length)
				: base(path, lastWriteTime)
			{
				this.FormattedFileLength = FormatFileLength(length);
			}

			private string FormatFileLength(long length)
			{
				if (length > 1024 * 1024 * 1024)
					return String.Format("{0:0.0} GB", (double)length / (1024 * 1024 * 1024));
				else if (length > 1024 * 1024)
					return String.Format("{0:0.0} MB", (double)length / (1024 * 1024));
				else if (length > 1024)
					return String.Format("{0:0.0} kB", (double)length / 1024);
				else
					return String.Format("{0} B", length);
			}

			public string FormattedFileLength { get; internal set; }
		}
#endif
	}
}
