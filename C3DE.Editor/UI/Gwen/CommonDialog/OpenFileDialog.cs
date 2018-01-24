using System;
using Gwen.Control;
using static global::Gwen.Platform.Platform;

namespace Gwen.CommonDialog
{
	/// <summary>
	/// Dialog for selecting an existing file.
	/// </summary>
	public class OpenFileDialog : FileDialog
	{
		public OpenFileDialog(ControlBase parent)
			: base(parent)
		{
		}

		protected override void OnCreated()
		{
			base.OnCreated();

			Title = "Open File";
			OkButtonText = "Open";
			EnableNewFolder = false;
		}

		protected override void OnItemSelected(string path)
		{
			if (FileExists(path))
			{
				SetCurrentItem(GetFileName(path));
			}
		}

		protected override bool IsSubmittedNameOk(string path)
		{
			if (DirectoryExists(path))
			{
				SetPath(path);
			}
			else if (FileExists(path))
			{
				return true;
			}

			return false;
		}

		protected override bool ValidateFileName(string path)
		{
			return FileExists(path);
		}
	}
}
