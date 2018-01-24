using System;
using Gwen.Control;
using static global::Gwen.Platform.Platform;

namespace Gwen.CommonDialog
{
	/// <summary>
	/// Dialog for selecting a file name for saving or creating.
	/// </summary>
	public class SaveFileDialog : FileDialog
	{
		public SaveFileDialog(ControlBase parent)
			: base(parent)
		{
		}

		protected override void OnCreated()
		{
			base.OnCreated();

			Title = "Save File";
			OkButtonText = "Save";
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
			if (DirectoryExists(path))
				return false;

			if (FileExists(path))
			{
				MessageBox win = MessageBox.Show(View, String.Format("File '{0}' already exists. Do you want to replace it?", GetFileName(path)), Title, MessageBoxButtons.YesNo);
				win.Dismissed += OnMessageBoxDismissed;
				win.UserData = path;
				return false;
			}

			return true;
		}

		private void OnMessageBoxDismissed(ControlBase sender, MessageBoxResultEventArgs args)
		{
			if (args.Result == MessageBoxResult.Yes)
				Close(sender.UserData as string);
		}
	}
}
