using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gwen.Control
{
	/// <summary>
	/// ListBox control.
	/// </summary>
	[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
	public class ListBox : ScrollControl
	{
		private readonly Table m_Table;
		private readonly List<ListBoxRow> m_SelectedRows;

		private bool m_MultiSelect;
		private bool m_IsToggle;

		/// <summary>
		/// Determines whether multiple rows can be selected at once.
		/// </summary>
		[Xml.XmlProperty]
		public bool AllowMultiSelect
		{
			get { return m_MultiSelect; }
			set
			{
				m_MultiSelect = value;
				if (value)
					IsToggle = true;
			}
		}

		/// <summary>
		/// Alternate row background colors.
		/// </summary>
		[Xml.XmlProperty]
		public bool AlternateColor { get { return m_Table.AlternateColor; } set { m_Table.AlternateColor = value; } }

		/// <summary>
		/// Determines whether rows can be unselected by clicking on them again.
		/// </summary>
		[Xml.XmlProperty]
		public bool IsToggle { get { return m_IsToggle; } set { m_IsToggle = value; } }

		/// <summary>
		/// Number of rows in the list box.
		/// </summary>
		public int RowCount { get { return m_Table.RowCount; } }

		/// <summary>
		/// Adjust the size of the control to fit all rows and columns.
		/// </summary>
		public bool AutoSizeColumnsToContent { get { return m_Table.AutoSizeColumnsToContent; } set { m_Table.AutoSizeColumnsToContent = value; } }

		/// <summary>
		/// Returns specific row of the ListBox.
		/// </summary>
		/// <param name="index">Row index.</param>
		/// <returns>Row at the specified index.</returns>
		public ListBoxRow this[int index] { get { return m_Table[index] as ListBoxRow; } }

		/// <summary>
		/// List of selected rows.
		/// </summary>
		public IEnumerable<ListBoxRow> SelectedRows { get { return m_SelectedRows; } }

		/// <summary>
		/// First selected row (and only if list is not multiselectable).
		/// </summary>
		public ListBoxRow SelectedRow
		{
			get
			{
				if (m_SelectedRows.Count == 0)
					return null;
				return m_SelectedRows[0];
			}
			set
			{
				if (m_Table.Children.Contains(value))
				{
					if (AllowMultiSelect)
					{
						SelectRow(value, false);
					}
					else
					{
						SelectRow(value, true);
					}
				}
			}
		}

		/// <summary>
		/// Gets the selected row number.
		/// </summary>
		[Xml.XmlProperty]
		public int SelectedRowIndex
		{
			get
			{
				var selected = SelectedRow;
				if (selected == null)
					return -1;
				return m_Table.GetRowIndex(selected);
			}
			set
			{
				SelectRow(value);
			}
		}

		/// <summary>
		/// Selected item.
		/// </summary>
		public object SelectedItem
		{
			get
			{
				var selected = SelectedRow;
				if (selected == null)
					return null;
				return selected.UserData;
			}
			set
			{
				SelectRow(value);
			}
		}

		/// <summary>
		/// Column count of table rows.
		/// </summary>
		[Xml.XmlProperty]
		public int ColumnCount { get { return m_Table.ColumnCount; } set { m_Table.ColumnCount = value; } }

		/// <summary>
		/// Collection of items. If the collection implements the INotifyCollectionChanged interface, items will be added and removed when the collection changes.
		/// </summary>
		public IEnumerable ItemsSource { get { return m_Table.ItemsSource; } set { m_Table.ItemsSource = value; } }

		/// <summary>
		/// Property name of the item to display. ListBox will be one column list.
		/// </summary>
		public string DisplayMember { get { return m_Table.DisplayMember; } set { m_Table.DisplayMember = value; } }

		/// <summary>
		/// Property names of the item to display. Each name represent a column on the list.
		/// </summary>
		public string[] DisplayMembers { get { return m_Table.DisplayMembers; } set { m_Table.DisplayMembers = value; } }

		/// <summary>
		/// Return a row factory used to create rows for this ListBox.
		/// </summary>
		public ITableRowFactory RowFactory { get { return m_Table.RowFactory; } set { m_Table.RowFactory = value; } }

		/// <summary>
		/// Invoked when a row has been selected.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<ItemSelectedEventArgs> RowSelected;

		/// <summary>
		/// Invoked whan a row has beed unselected.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<ItemSelectedEventArgs> RowUnselected;

		/// <summary>
		/// Invoked whan a row has beed double clicked.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<ItemSelectedEventArgs> RowDoubleClicked;

		/// <summary>
		/// Initializes a new instance of the <see cref="ListBox"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ListBox(ControlBase parent, ITableRowFactory rowFactory = null)
			: base(parent)
		{
			MinimumSize = new Size(16, 16);
			Padding = Padding.One;

			m_SelectedRows = new List<ListBoxRow>();

			MouseInputEnabled = true;
			EnableScroll(false, true);
			AutoHideBars = true;

			m_Table = new Table(this);
			m_Table.RowFactory = rowFactory != null ? rowFactory : new ListRowFactory(this);
			m_Table.AutoSizeColumnsToContent = true;
			m_Table.ColumnCount = 1;

			m_MultiSelect = false;
			m_IsToggle = false;
		}

		/// <summary>
		/// Selects the specified row by index.
		/// </summary>
		/// <param name="index">Row to select.</param>
		/// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
		public void SelectRow(int index, bool clearOthers = false)
		{
			if (index < 0 || index >= m_Table.RowCount)
				return;

			SelectRow(m_Table.Children[index], clearOthers);
		}

		/// <summary>
		/// Selects the specified row(s) by text.
		/// </summary>
		/// <param name="rowText">Text to search for (exact match).</param>
		/// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
		public void SelectRows(string rowText, bool clearOthers = false)
		{
			var rows = m_Table.Children.OfType<ListBoxRow>().Where(x => x.Text == rowText);
			foreach (ListBoxRow row in rows)
			{
				SelectRow(row, clearOthers);
			}
		}

		/// <summary>
		/// Selects the specified row(s) by regex text search.
		/// </summary>
		/// <param name="pattern">Regex pattern to search for.</param>
		/// <param name="regexOptions">Regex options.</param>
		/// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
		public void SelectRowsByRegex(string pattern, RegexOptions regexOptions = RegexOptions.None, bool clearOthers = false)
		{
			var rows = m_Table.Children.OfType<ListBoxRow>().Where(x => Regex.IsMatch(x.Text, pattern));
			foreach (ListBoxRow row in rows)
			{
				SelectRow(row, clearOthers);
			}
		}

		/// <summary>
		/// Selects the specified row(s) by internal name.
		/// </summary>
		/// <param name="name">The internal name to look for.</param>
		/// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
		public void SelectByName(string name, bool clearOthers = false)
		{
			var rows = m_Table.Children.OfType<ListBoxRow>().Where(x => x.Name == name);
			foreach (ListBoxRow row in rows)
			{
				SelectRow(row, clearOthers);
			}
		}

		/// <summary>
		/// Selects the specified row(s) by user data.
		/// </summary>
		/// <param name="userData">The UserData to look for. The equivalency check uses "param.Equals(item.UserData)".
		/// If null is passed in, it will look for null/unset UserData.</param>
		/// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
		public void SelectByUserData(object userData, bool clearOthers = false)
		{
			var rows = m_Table.Children.OfType<ListBoxRow>().Where(x => (x.UserData == null && userData == null) || userData.Equals(x.UserData));
			foreach (ListBoxRow row in rows)
			{
				SelectRow(row, clearOthers);
			}
		}

		/// <summary>
		/// Selects row(s) by specified item.
		/// </summary>
		/// <param name="item">Item to select.</param>
		/// <param name="clearOthers">Determines whether to deselect previously selected rows.</param>
		public void SelectRow(object item, bool clearOthers = false)
		{
			var rows = m_Table.Children.OfType<ListBoxRow>().Where(x => x.UserData == item);
			foreach (ListBoxRow row in rows)
			{
				SelectRow(row, clearOthers);
			}
		}

		private void SelectRow(ControlBase control, bool clearOthers = false)
		{
			if (!AllowMultiSelect || clearOthers)
				UnselectAll();

			ListBoxRow row = control as ListBoxRow;
			if (row == null)
				return;

			row.IsSelected = true;
			m_SelectedRows.Add(row);
			if (RowSelected != null)
				RowSelected.Invoke(this, new ItemSelectedEventArgs(row));
		}

		/// <summary>
		/// Deselects all rows.
		/// </summary>
		public virtual void UnselectAll()
		{
			foreach (ListBoxRow row in m_SelectedRows)
			{
				row.IsSelected = false;
				if (RowUnselected != null)
					RowUnselected.Invoke(this, new ItemSelectedEventArgs(row));
			}
			m_SelectedRows.Clear();
		}

		/// <summary>
		/// Unselects the specified row.
		/// </summary>
		/// <param name="row">Row to unselect.</param>
		public void UnselectRow(ListBoxRow row)
		{
			row.IsSelected = false;
			m_SelectedRows.Remove(row);

			if (RowUnselected != null)
				RowUnselected.Invoke(this, new ItemSelectedEventArgs(row));
		}

		/// <summary>
		/// Adds a new row.
		/// </summary>
		/// <param name="text">Row text. If null, create an empty row.</param>
		/// <param name="name">Internal control name.</param>
		/// <param name="userData">User data for newly created row</param>
		/// <returns>Newly created control.</returns>
		public ListBoxRow AddRow(string text = null, string name = "", object userData = null)
		{
			var row = m_Table.AddRow(text, name, userData);
			if (row is ListBoxRow)
				return row as ListBoxRow;
			else
				throw new InvalidCastException("ListBox row factory returned row that is not a ListBoxRow.");
		}

		/// <summary>
		/// Add row.
		/// </summary>
		/// <param name="row">Row.</param>
		public void AddRow(ListBoxRow row)
		{
			m_Table.AddRow(row);

			row.Selected += OnRowSelected;
			row.DoubleClicked += OnRowDoubleClicked;
		}

		/// <summary>
		/// Adds a new row with specified item.
		/// </summary>
		/// <param name="item">Item to add.</param>
		/// <returns>New row.</returns>
		public ListBoxRow AddRow(object item)
		{
			var row = m_Table.AddRow(item);
			if (row is ListBoxRow)
				return row as ListBoxRow;
			else
				throw new InvalidCastException("ListBox row factory returned row that is not a ListBoxRow.");
		}

		/// <summary>
		/// Insert a new row to specified index.
		/// </summary>
		/// <param name="index">Index where to insert.</param>
		/// <param name="text">Row text. If null, create an empty row.</param>
		/// <param name="name">Internal control name.</param>
		/// <param name="userData">User data for newly created row</param>
		/// <returns>Newly created row.</returns>
		public ListBoxRow InsertRow(int index, string text = null, string name = "", object userData = null)
		{
			var row = m_Table.InsertRow(index, text, name, userData);
			if (row is ListBoxRow)
				return row as ListBoxRow;
			else
				throw new InvalidCastException("ListBox row factory returned row that is not a ListBoxRow.");
		}

		/// <summary>
		/// Insert a row to specified index.
		/// </summary>
		/// <param name="index">Index where to insert.</param>
		/// <param name="row">Row.</param>
		public void InsertRow(int index, ListBoxRow row)
		{
			m_Table.InsertRow(index, row);

			row.Selected += OnRowSelected;
			row.DoubleClicked += OnRowDoubleClicked;
		}

		/// <summary>
		/// Insert a new row with specified item to specified index.
		/// </summary>
		/// <param name="index">Index where to insert.</param>
		/// <param name="item">Item to add.</param>
		/// <returns>New row.</returns>
		public ListBoxRow InsertRow(int index, object item)
		{
			var row = m_Table.InsertRow(index, item);
			if (row is ListBoxRow)
				return row as ListBoxRow;
			else
				throw new InvalidCastException("ListBox row factory returned row that is not a ListBoxRow.");
		}

		/// <summary>
		/// Removes a row by reference.
		/// </summary>
		/// <param name="row">Row to remove.</param>
		public void RemoveRow(TableRow row)
		{
			m_Table.RemoveRow(row);
		}

		/// <summary>
		/// Removes the specified row by index.
		/// </summary>
		/// <param name="idx">Row index.</param>
		public void RemoveRow(int idx)
		{
			m_Table.RemoveRow(idx);
		}

		/// <summary>
		/// Remove row by item.
		/// </summary>
		/// <param name="item">Item to remove.</param>
		public void RemoveRow(object item)
		{
			m_Table.RemoveRow(item);
		}

		/// <summary>
		/// Removes all rows.
		/// </summary>
		public virtual void Clear()
		{
			UnselectAll();
			m_Table.Clear();
		}

		/// <summary>
		/// Gets the index of a specified row.
		/// </summary>
		/// <param name="row">Row to search for.</param>
		/// <returns>Row index if found, -1 otherwise.</returns>
		public int GetRowIndex(ListBoxRow row)
		{
			return m_Table.GetRowIndex(row);
		}

		/// <summary>
		/// Sizes to fit contents.
		/// </summary>
		public void SizeToContent(int maxWidth = 0)
		{
			m_Table.SizeColumnsToContent(maxWidth);
		}

		/// <summary>
		/// Sets the column width (in pixels).
		/// </summary>
		/// <param name="column">Column index.</param>
		/// <param name="width">Column width.</param>
		public void SetColumnWidth(int column, int width)
		{
			m_Table.SetColumnWidth(column, width);
		}

		/// <summary>
		/// Gets the column width (in pixels).
		/// </summary>
		/// <param name="columnIndex">Column index.</param>
		/// <returns>Column width.</returns>
		public int GetColumnWidth(int columnIndex)
		{
			return m_Table.GetColumnWidth(columnIndex);
		}

		/// <summary>
		/// Sizes to fit contents.
		/// </summary>
		public void SizeColumnsToContent(int maxWidth = 0)
		{
			m_Table.SizeColumnsToContent(maxWidth);
		}

		/// <summary>
		/// Handler for the row selection event.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected virtual void OnRowSelected(ControlBase control, ItemSelectedEventArgs args)
		{
			ListBoxRow row = args.SelectedItem as ListBoxRow;
			if (row == null)
				return;

			if (row.IsSelected)
			{
				if (IsToggle)
					UnselectRow(row);
			}
			else
			{
				SelectRow(row, false);
			}
		}

		/// <summary>
		/// Handler for the row double click event.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected virtual void OnRowDoubleClicked(ControlBase control, ClickedEventArgs args)
		{
			ListBoxRow row = control as ListBoxRow;
			if (row == null)
				return;

			if (RowDoubleClicked != null)
				RowDoubleClicked.Invoke(this, new ItemSelectedEventArgs(row));
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			skin.DrawListBox(this);
		}

		internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent)
		{
			ListBox element = new ListBox(parent);
			parser.ParseAttributes(element);
			if (parser.MoveToContent())
			{
				foreach (string elementName in parser.NextElement())
				{
					if (elementName == "Row")
					{
						element.AddRow(parser.ParseElement<ListBoxRow>(element));
					}
				}
			}
			return element;
		}

		public class ListRowFactory : Table.TableRowFactory
		{
			private ListBox m_ListBox;

			public ListRowFactory(ListBox listBox)
				: base(listBox.m_Table)
			{
				m_ListBox = listBox;
			}

			protected override TableRow CreateRow()
			{
				ListBoxRow row = new ListBoxRow(Table) { ColumnCount = m_ListBox.ColumnCount };
				row.Selected += m_ListBox.OnRowSelected;
				row.DoubleClicked += m_ListBox.OnRowDoubleClicked;
				return row;
			}
		}
	}
}
