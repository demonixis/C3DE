using System;
using System.Collections.Generic;
using System.IO;

namespace Gwen.Platform
{
	public interface IFileSystemItemInfo
	{
		string Name { get; }
		string FullName { get; }
		string FormattedLastWriteTime { get; }
	}

	public interface IFileSystemDirectoryInfo : IFileSystemItemInfo
	{

	}

	public interface IFileSystemFileInfo : IFileSystemItemInfo
	{
		string FormattedFileLength { get; }
	}

	public interface ISpecialFolder
	{
		string Name { get; }
		string Category { get; }
		string Path { get; }
	}

	/// <summary>
	/// Platform specific functions.
	/// </summary>
	public interface IPlatform
	{
		/// <summary>
		/// Gets text from clipboard.
		/// </summary>
		/// <returns>Clipboard text.</returns>
		string GetClipboardText();

		/// <summary>
		/// Sets the clipboard text.
		/// </summary>
		/// <param name="text">Text to set.</param>
		/// <returns>True if succeeded.</returns>
		bool SetClipboardText(string text);

		/// <summary>
		/// Gets elapsed time. Initialization time is platform specific.
		/// </summary>
		/// <returns>Time interval in seconds.</returns>
		float GetTimeInSeconds();

		/// <summary>
		/// Changes the mouse cursor.
		/// </summary>
		/// <param name="cursor">Cursor type.</param>
		void SetCursor(Cursor cursor);

		/// <summary>
		/// Get special folders of the system.
		/// </summary>
		/// <returns>List of folders.</returns>
		IEnumerable<ISpecialFolder> GetSpecialFolders();

		string GetFileName(string path);
		string GetDirectoryName(string path);

		bool FileExists(string path);
		bool DirectoryExists(string path);

		void CreateDirectory(string path);

		string Combine(string path1, string path2);
		string Combine(string path1, string path2, string path3);
		string Combine(string path1, string path2, string path3, string path4);

		string CurrentDirectory { get; }

		IEnumerable<IFileSystemDirectoryInfo> GetDirectories(string path);
		IEnumerable<IFileSystemFileInfo> GetFiles(string path, string filter);

		Stream GetFileStream(string path, bool isWritable);
	}
}
