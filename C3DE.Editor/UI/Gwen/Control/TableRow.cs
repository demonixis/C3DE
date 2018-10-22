using System;

namespace Gwen.Control
{
	/// <summary>
	/// Single table row.
	/// </summary>
	public class TableRow : ControlBase
	{
		private int m_ColumnCount;
		private bool m_EvenRow;
		private TableCell[] m_Columns;

		/// <summary>
		/// Text of the first column.
		/// </summary>
		[Xml.XmlProperty]
		public string Text { get { return GetCellText(0); } set { SetCellText(0, value); } }

		internal int ColumnCount { get { return m_ColumnCount; } set { SetColumnCount(value); } }

		internal bool EvenRow { get { return m_EvenRow; } set { m_EvenRow = value; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="TableRow"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public TableRow(ControlBase parent)
			: base(parent)
		{
			KeyboardInputEnabled = true;
		}

		/// <summary>
		/// Sets the number of columns.
		/// </summary>
		/// <param name="columnCount">Number of columns.</param>
		protected void SetColumnCount(int columnCount)
		{
			if (columnCount == m_ColumnCount) return;

			TableCell[] newColumns = new TableCell[columnCount];

			for (int i = 0; i < m_ColumnCount; i++)
			{
				if (i < columnCount)
				{
					newColumns[i] = m_Columns[i];
				}
				else if (m_Columns[i].Control != null)
				{
					RemoveChild(m_Columns[i].Control, true);
					m_Columns[i].Control = null;
				}
			}

			m_ColumnCount = columnCount;
			m_Columns = newColumns;
		}

		/// <summary>
		/// Sets the text of a specified cell.
		/// </summary>
		/// <param name="columnIndex">Column number.</param>
		/// <param name="text">Text to set.</param>
		public void SetCellText(int columnIndex, string text)
		{
			if (columnIndex >= m_ColumnCount)
				throw new ArgumentOutOfRangeException("columnIndex");

			if (m_Columns[columnIndex].Control != null && !(m_Columns[columnIndex].Control is Label))
			{
				RemoveChild(m_Columns[columnIndex].Control, true);
				m_Columns[columnIndex].Control = null;
			}

			if (m_Columns[columnIndex].Control == null)
			{
				Label label = new Label(this);
				label.Padding = Padding.Three;
				label.TextColor = Skin.Colors.ListBox.Text_Normal;
				m_Columns[columnIndex].Control = label;
			}

			((Label)m_Columns[columnIndex].Control).Text = text;
		}

		/// <summary>
		/// Returns text of a specified row cell (default first).
		/// </summary>
		/// <param name="columnIndex">Column index.</param>
		/// <returns>Column cell text.</returns>
		public string GetCellText(int columnIndex = 0)
		{
			if (columnIndex >= m_ColumnCount)
				throw new ArgumentOutOfRangeException("columnIndex");

			if (m_Columns[columnIndex].Control == null || !(m_Columns[columnIndex].Control is Label))
				return null;

			return ((Label)m_Columns[columnIndex].Control).Text;
		}

		/// <summary>
		/// Sets the text color for all cells.
		/// </summary>
		/// <param name="color">Text color.</param>
		public void SetTextColor(Color color)
		{
			for (int i = 0; i < m_ColumnCount; i++)
			{
				if (m_Columns[i].Control == null || !(m_Columns[i].Control is Label))
					continue;
				((Label)m_Columns[i].Control).TextColor = color;
			}
		}

		/// <summary>
		/// Sets the contents of a specified cell.
		/// </summary>
		/// <param name="columnIndex">Column number.</param>
		/// <param name="control">Cell contents.</param>
		public void SetCellContents(int columnIndex, ControlBase control)
		{
			if (columnIndex >= m_ColumnCount)
				throw new ArgumentOutOfRangeException("columnIndex");

			if (m_Columns[columnIndex].Control != null)
			{
				RemoveChild(m_Columns[columnIndex].Control, true);
			}

			m_Columns[columnIndex].Control = control;
			control.Parent = this;
		}

		/// <summary>
		/// Gets the contents of a specified cell.
		/// </summary>
		/// <param name="columnIndex">Column number.</param>
		/// <returns>Control embedded in the cell.</returns>
		public ControlBase GetCellContents(int columnIndex)
		{
			if (columnIndex >= m_ColumnCount)
				throw new ArgumentOutOfRangeException("columnIndex");

			return m_Columns[columnIndex].Control;
		}

		protected override Size OnMeasure(Size availableSize)
		{
			int width = 0;
			int height = 0;

			for (int i = 0; i < m_ColumnCount; i++)
			{
				width += m_Columns[i].Width;

				ControlBase control = m_Columns[i].Control;
				if (control == null)
					continue;

				control.Measure(new Size(m_Columns[i].Width, availableSize.Height));
				height = Math.Max(height, control.MeasuredSize.Height);
			}

			return new Size(width, height);
		}

		protected override Size OnArrange(Size finalSize)
		{
			int x = 0;
			int height = 0;

			for (int i = 0; i < m_ColumnCount; i++)
			{
				ControlBase control = m_Columns[i].Control;
				if (control != null)
				{
					if (i == m_ColumnCount - 1)
						control.Arrange(new Rectangle(x, 0, Math.Max(0, finalSize.Width - x), control.MeasuredSize.Height));
					else
						control.Arrange(new Rectangle(x, 0, m_Columns[i].Width, control.MeasuredSize.Height));
					height = Math.Max(height, control.MeasuredSize.Height);
				}
				x += m_Columns[i].Width;
			}

			return new Size(finalSize.Width, height);
		}

		/// <summary>
		/// Handler for Copy event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void OnCopy(ControlBase from, EventArgs args)
		{
			Platform.Platform.SetClipboardText(Text);
		}

		internal void SetColumnWidth(int columnIndex, int width)
		{
			m_Columns[columnIndex].Width = width;
		}

		internal ControlBase GetCell(int index)
		{
			return m_Columns[index].Control;
		}

		private struct TableCell
		{
			public ControlBase Control;
			public int Width;
		}
	}
}
