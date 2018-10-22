using System;
using System.Linq;
using Gwen.Control;
using Gwen.Xml;
using Gwen.Platform;
using static global::Gwen.Platform.Platform;

namespace Gwen.CommonDialog
{
	/// <summary>
	/// Base class for a file or directory dialog.
	/// </summary>
	public abstract class FileDialog : Component
	{
		private Action<string> m_Callback;

		private string m_CurrentFolder;
		private string m_CurrentFilter;

		private bool m_FoldersOnly;

		private bool m_OnClosing;

		private TreeControl m_Folders;
		private ListBox m_Items;
		private TextBox m_Path;
		private TextBox m_SelectedName;
		private ComboBox m_Filters;
		private Button m_Ok;
		private Button m_NewFolder;
		private VerticalSplitter m_NameFilterSplitter;
		private Label m_FileNameLabel;
		private Window m_Window;

		/// <summary>
		/// Initial folder for the dialog.
		/// </summary>
		public string InitialFolder { set { SetPath(value); } }

		/// <summary>
		/// Set initial folder and selected item.
		/// </summary>
		public string CurrentItem { set { SetPath(GetDirectoryName(value)); SetCurrentItem(GetFileName(value)); } }

		/// <summary>
		/// Window title.
		/// </summary>
		public string Title { get { return m_Window.Title; } set { m_Window.Title = value; } }

		/// <summary>
		/// File filters. See <see cref="SetFilters(string, int)"/>.
		/// </summary>
		public string Filters { set { SetFilters(value); } }

		/// <summary>
		/// Text shown in the ok button.
		/// </summary>
		public string OkButtonText { get { return m_Ok.Text; } set { m_Ok.Text = value; } }

		/// <summary>
		/// Function that is called when dialog is closed. If ok is pressed, parameter is the selected file / directory.
		/// If cancel is pressed or window closed, parameter is null.
		/// </summary>
		public Action<string> Callback { get { return m_Callback; } set { m_Callback = value; } }

		/// <summary>
		/// Hide or show new folder button.
		/// </summary>
		public bool EnableNewFolder { get { return !m_NewFolder.IsCollapsed; } set { m_NewFolder.IsCollapsed = !value; } }

		/// <summary>
		/// Show only directories.
		/// </summary>
		protected bool FoldersOnly
		{
			get { return m_FoldersOnly; }
			set
			{
				m_FoldersOnly = value;
				m_Filters.IsCollapsed = value;
				m_FileNameLabel.Text = "Folder name:";
				if (value)
					m_NameFilterSplitter.Zoom(0);
				else
					m_NameFilterSplitter.UnZoom();
			}
		}
				
		/// <summary>
		/// Constructor for the base class. Implementing classes must call this.
		/// </summary>
		/// <param name="parent">Parent.</param>
		protected FileDialog(ControlBase parent)
			: base(parent, new XmlStringSource(Xml))
		{
		}

		protected override void OnCreated()
		{
			m_Window = View as Window;
			m_Folders = GetControl<TreeControl>("Folders");
			m_Items = GetControl<ListBox>("Items");
			m_Path = GetControl<TextBox>("Path");
			m_SelectedName = GetControl<TextBox>("SelectedName");
			m_Filters = GetControl<ComboBox>("Filters");
			m_Ok = GetControl<Button>("Ok");
			m_NewFolder = GetControl<Button>("NewFolder");
			m_NameFilterSplitter = GetControl<VerticalSplitter>("NameFilterSplitter");
			m_FileNameLabel = GetControl<Label>("FileNameLabel");

			UpdateFolders();

			m_OnClosing = false;

			m_CurrentFolder = CurrentDirectory;

			m_CurrentFilter = "*.*";
			m_Filters.AddItem("All files (*.*)", "All files (*.*)", "*.*");
		}

		/// <summary>
		/// Set current path.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <returns>True if the path change was successful. False otherwise.</returns>
		public bool SetPath(string path)
		{
			if (DirectoryExists(path))
			{
				m_CurrentFolder = path;
				m_Path.Text = m_CurrentFolder;
				UpdateItemList();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Set filters.
		/// </summary>
		/// <param name="filterStr">Filter string. Format 'name|filter[|name|filter]...'</param>
		/// <param name="current">Set this index as a current filter.</param>
		public void SetFilters(string filterStr, int current = 0)
		{
			string[] filters = filterStr.Split('|');
			if ((filters.Length & 0x1) == 0x1)
				throw new Exception("Error in filter.");

			m_Filters.RemoveAll();

			for (int i = 0; i < filters.Length; i += 2)
			{
				m_Filters.AddItem(filters[i], filters[i], filters[i + 1]);
			}

			m_Filters.SelectedIndex = current;
		}

		/// <summary>
		/// Set current file or directory.
		/// </summary>
		/// <param name="item">File or directory. This doesn't need to exists.</param>
		protected void SetCurrentItem(string item)
		{
			m_SelectedName.Text = item;
		}

		/// <summary>
		/// Close the dialog and call the call back function.
		/// </summary>
		/// <param name="path">Parameter for the call back function.</param>
		protected void Close(string path)
		{
			OnClosing(path, true);
		}

		/// <summary>
		/// Called when the user selects a file or directory.
		/// </summary>
		/// <param name="path">Full path of selected file or directory.</param>
		protected virtual void OnItemSelected(string path)
		{
			if ((DirectoryExists(path) && m_FoldersOnly) || (FileExists(path) && !m_FoldersOnly))
			{
				SetCurrentItem(GetFileName(path));
			}
		}

		/// <summary>
		/// Called to validate the file or directory name when the user enters it.
		/// </summary>
		/// <param name="path">Full path of the name.</param>
		/// <returns>Is the name valid.</returns>
		protected virtual bool IsSubmittedNameOk(string path)
		{
			if (DirectoryExists(path))
			{
				if (!m_FoldersOnly)
				{
					SetPath(path);
				}
			}
			else if (FileExists(path))
			{
				return true;
			}
			else
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Called to validate the path when the user presses the ok button.
		/// </summary>
		/// <param name="path">Full path.</param>
		/// <returns>Is the path valid.</returns>
		protected virtual bool ValidateFileName(string path)
		{
			return true;
		}

		/// <summary>
		/// Called when the dialog is closing.
		/// </summary>
		/// <param name="path">Path for the call back function</param>
		/// <param name="doClose">True if the dialog needs to be closed.</param>
		protected virtual void OnClosing(string path, bool doClose)
		{
			if (m_OnClosing)
				return;

			m_OnClosing = true;

			if (doClose)
				m_Window.Close();

			if (m_Callback != null)
				m_Callback(path);
		}

		private void OnPathSubmitted(ControlBase sender, EventArgs args)
		{
			if (!SetPath(m_Path.Text))
			{
				m_Path.Text = m_CurrentFolder;
			}
		}

		private void OnUpClicked(ControlBase sender, ClickedEventArgs args)
		{
			string newPath = GetDirectoryName(m_CurrentFolder);
			if (newPath != null)
			{
				SetPath(newPath);
			}
		}

		private void OnNewFolderClicked(ControlBase sender, ClickedEventArgs args)
		{
			string path = m_Path.Text;
			if (DirectoryExists(path))
			{
				m_Path.Focus();
			}
			else
			{
				try
				{
					CreateDirectory(path);
					SetPath(path);
				}
				catch (Exception ex)
				{
					MessageBox.Show(View, ex.Message, Title, MessageBoxButtons.OK);
				}
			}
		}

		private void OnFolderSelected(ControlBase sender, EventArgs args)
		{
			TreeNode node = sender as TreeNode;
			if (node != null && node.UserData != null)
			{
				SetPath(node.UserData as string);
			}
		}

		private void OnItemSelected(ControlBase sender, ItemSelectedEventArgs args)
		{
			string path = args.SelectedItem.UserData as string;
			if (path != null)
			{
				OnItemSelected(path);
			}
		}

		private void OnItemDoubleClicked(ControlBase sender, ItemSelectedEventArgs args)
		{
			string path = args.SelectedItem.UserData as string;
			if (path != null)
			{
				if (DirectoryExists(path))
				{
					SetPath(path);
				}
				else
				{
					OnOkClicked(null, null);
				}
			}
		}

		private void OnNameSubmitted(ControlBase sender, EventArgs args)
		{
			string path = Combine(m_CurrentFolder, m_SelectedName.Text);
			if (IsSubmittedNameOk(path))
				OnOkClicked(null, null);
		}

		private void OnFilterSelected(ControlBase sender, ItemSelectedEventArgs args)
		{
			m_CurrentFilter = m_Filters.SelectedItem.UserData as string;
			UpdateItemList();
		}

		private void OnOkClicked(ControlBase sender, ClickedEventArgs args)
		{
			string path = Combine(m_CurrentFolder, m_SelectedName.Text);
			if (ValidateFileName(path))
			{
				OnClosing(path, true);
			}
		}

		private void OnCancelClicked(ControlBase sender, ClickedEventArgs args)
		{
			OnClosing(null, true);
		}

		private void OnWindowClosed(ControlBase sender, EventArgs args)
		{
			OnClosing(null, false);
		}

		private void UpdateItemList()
		{
			m_Items.Clear();

			IOrderedEnumerable<IFileSystemDirectoryInfo> directories;
			IOrderedEnumerable<IFileSystemFileInfo> files = null;
			try
			{
				directories = GetDirectories(m_CurrentFolder).OrderBy(di => di.Name);
				if (!m_FoldersOnly)
					files = GetFiles(m_CurrentFolder, m_CurrentFilter).OrderBy(fi => fi.Name);
			}
			catch (Exception ex)
			{
				MessageBox.Show(View, ex.Message, Title, MessageBoxButtons.OK);
				return;
			}

			foreach (IFileSystemDirectoryInfo di in directories)
			{
				ListBoxRow row = m_Items.AddRow(di.Name, null, di.FullName);
				row.SetCellText(1, "<dir>");
				row.SetCellText(2, di.FormattedLastWriteTime);
			}

			if (!m_FoldersOnly)
			{
				foreach (IFileSystemFileInfo fi in files)
				{
					ListBoxRow row = m_Items.AddRow(fi.Name, null, fi.FullName);
					row.SetCellText(1, fi.FormattedFileLength);
					row.SetCellText(2, fi.FormattedFileLength);
				}
			}
		}

		private void UpdateFolders()
		{
			m_Folders.RemoveAllNodes();

			foreach (ISpecialFolder folder in Platform.Platform.GetSpecialFolders())
			{
				TreeNode category = m_Folders.FindNodeByName(folder.Category, false);
				if (category == null)
					category = m_Folders.AddNode(folder.Category, folder.Category, null);

				category.AddNode(folder.Name, folder.Name, folder.Path);
			}

			m_Folders.ExpandAll();
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

		private string FormatFileTime(DateTime dateTime)
		{
			return "";
			//return String.Format("{0} {1}", dateTime.ToShortDateString(), dateTime.ToLongTimeString());
		}

		private static readonly string Xml = @"<?xml version='1.0' encoding='UTF-8'?>
			<Window Size='400,300' StartPosition='CenterCanvas' Closed='OnWindowClosed'>
				<DockLayout Margin='2' >
					<DockLayout Dock='Top'>
						<Label Dock='Left' Margin='2' Alignment='CenterV,Left' Text='Path:' />
						<TextBox Name='Path' Margin='2' Dock='Fill' SubmitPressed='OnPathSubmitted' />
						<Button Name='NewFolder' Margin='2' Dock='Right' Padding='10,0,10,0' Text='New' Clicked='OnNewFolderClicked' />
						<Button Name='Up' Margin='2' Dock='Right' Padding='10,0,10,0' Text='Up' Clicked='OnUpClicked' />
					</DockLayout>
					<VerticalSplitter Dock='Fill' Value='0.3' SplitterSize='2'>
						<TreeControl Name='Folders' Margin='2' Selected='OnFolderSelected' />
						<ListBox Name='Items' Margin='2' ColumnCount='3' RowSelected='OnItemSelected' RowDoubleClicked='OnItemDoubleClicked' />
					</VerticalSplitter>
					<DockLayout Dock='Bottom'>
						<Button Name='Cancel' Margin='2' Dock='Right' Padding='10,0,10,0' Width='100' Text='Cancel' Clicked='OnCancelClicked' />
						<Button Name='Ok' Margin='2' Dock='Right' Padding='10,0,10,0' Width='100' Text='Ok' Clicked='OnOkClicked' />
					</DockLayout>
					<VerticalSplitter Name='NameFilterSplitter' Dock='Bottom' Value='0.7' SplitterSize='2'>
						<DockLayout>
							<Label Name='FileNameLabel' Dock='Left' Margin='2' Alignment='CenterV,Left' Text='File name:' />
							<TextBox Name='SelectedName' Dock='Fill' Margin='2' SubmitPressed='OnNameSubmitted'/>
						</DockLayout>
						<ComboBox Name='Filters' Margin='2' ItemSelected='OnFilterSelected'/>
					</VerticalSplitter>
				</DockLayout>
			</Window>
			";
	}
}
