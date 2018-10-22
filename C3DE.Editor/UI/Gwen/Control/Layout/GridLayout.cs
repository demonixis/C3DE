using System;
using System.Collections.Generic;

namespace Gwen.Control.Layout
{
	/// <summary>
	/// GridLayout column widths or row heights.
	/// </summary>
	/// <remarks>
	/// Cell size can be one of
	/// a) Single.NaN: Auto sized. Size is the smallest size the control can be drawn.
	/// b) 0.0 - 1.0: Remaining space filled proportionally.
	/// c) More than 1.0: Absolute cell size.
	/// </remarks>
	public class GridCellSizes : List<float>
	{
		public GridCellSizes(IEnumerable<float> sizes)
			: base(sizes)
		{
		}

		public GridCellSizes(int count)
			: base(count)
		{
		}

		public GridCellSizes(params float[] sizes)
			: base(sizes)
		{
		}
	}

	/// <summary>
	/// Arrange child controls into columns and rows by adding them in column and row order.
	/// Add every column of the first row, then every column of the second row etc.
	/// </summary>
	[Xml.XmlControl]
	public class GridLayout : ControlBase
	{
		private int m_ColumnCount;

		private float[] m_RequestedColumnWidths;
		private float[] m_RequestedRowHeights;

		private Size m_TotalFixedSize;
		private Size m_TotalAutoFixedSize;

		private int[] m_ColumnWidths;
		private int[] m_RowHeights;

		public const float AutoSize = Single.NaN;
		public const float Fill = 1.0f;

		/// <summary>
		/// Number of columns. This can be used when all cells are auto size.
		/// </summary>
		[Xml.XmlProperty]
		public int ColumnCount { get { return m_ColumnCount; } set { m_ColumnCount = value; Invalidate(); } }

		/// <summary>
		/// Column widths. <see cref="GridCellSizes"/>
		/// </summary>
		[Xml.XmlProperty]
		public GridCellSizes ColumnWidths { set { SetColumnWidths(value.ToArray()); } }

		/// <summary>
		/// Row heights. <see cref="GridCellSizes"/>
		/// </summary>
		[Xml.XmlProperty]
		public GridCellSizes RowHeights { set { SetRowHeights(value.ToArray()); } }

		/// <summary>
		/// Initializes a new instance of the <see cref="GridLayout"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public GridLayout(ControlBase parent)
			: base(parent)
		{
			m_ColumnCount = 1;
		}

		/// <summary>
		/// Set column widths. <see cref="GridCellSizes"/>
		/// </summary>
		/// <param name="widths">Array of widths.</param>
		public void SetColumnWidths(params float[] widths)
		{
			m_TotalFixedSize.Width = 0;
			float relTotalWidth = 0.0f;
			foreach (float w in widths)
			{
				if (w >= 0.0f && w <= 1.0f)
					relTotalWidth += w;
				else if (w > 1.0f)
					m_TotalFixedSize.Width += (int)w;
			}

			if (relTotalWidth > 1.0f)
				throw new ArgumentException("Relative widths exceed total value of 1.0 (100%).");

			m_RequestedColumnWidths = widths;
			m_ColumnCount = widths.Length;
			Invalidate();
		}

		/// <summary>
		/// Set row heights. <see cref="GridCellSizes"/>
		/// </summary>
		/// <param name="heights">Array of heights.</param>
		public void SetRowHeights(params float[] heights)
		{
			m_TotalFixedSize.Height = 0;
			float relTotalHeight = 0.0f;
			foreach (float h in heights)
			{
				if (h >= 0.0f && h <= 1.0f)
					relTotalHeight += h;
				else if (h > 1.0f)
					m_TotalFixedSize.Height += (int)h;
			}

			if (relTotalHeight > 1.0f)
				throw new ArgumentException("Relative heights exceed total value of 1.0 (100%).");

			m_RequestedRowHeights = heights;
			Invalidate();
		}

		protected override Size OnMeasure(Size availableSize)
		{
			availableSize -= Padding;

			if (m_ColumnWidths == null || m_ColumnWidths.Length != m_ColumnCount)
				m_ColumnWidths = new int[m_ColumnCount];

			int rowCount = (this.Children.Count + m_ColumnCount - 1) / m_ColumnCount;
			if (m_RowHeights == null || m_RowHeights.Length != rowCount)
				m_RowHeights = new int[rowCount];

			int columnIndex;
			for (columnIndex = 0; columnIndex < m_ColumnCount; columnIndex++)
			{
				m_ColumnWidths[columnIndex] = 0;
			}

			int rowIndex;
			for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				m_RowHeights[rowIndex] = 0;
			}

			Size cellAvailableSize = availableSize;
			columnIndex = 0;
			rowIndex = 0;
			foreach (ControlBase child in this.Children)
			{
				Size size;
				if (child.IsCollapsed)
				{
					size = Size.Zero;
				}
				else
				{
					size = cellAvailableSize;
					if (m_RequestedColumnWidths != null)
					{
						float w = m_RequestedColumnWidths[columnIndex];
						if (w >= 0.0f && w <= 1.0f)
							size.Width = (int)(w * (availableSize.Width - m_TotalFixedSize.Width));
						else if (w > 1.0f)
							size.Width = (int)w;
					}
					if (m_RequestedRowHeights != null)
					{
						float h = m_RequestedRowHeights[rowIndex];
						if (h >= 0.0f && h <= 1.0f)
							size.Height = (int)(h * (availableSize.Height - m_TotalFixedSize.Height));
						else if (h > 1.0f)
							size.Height = (int)h;
					}

					size = child.Measure(size);
				}

				if (m_ColumnWidths[columnIndex] < size.Width)
					m_ColumnWidths[columnIndex] = size.Width;

				if (m_RowHeights[rowIndex] < size.Height)
					m_RowHeights[rowIndex] = size.Height;

				cellAvailableSize.Width -= m_ColumnWidths[columnIndex];

				columnIndex++;
				if (columnIndex == m_ColumnCount)
				{
					cellAvailableSize.Width = availableSize.Width;
					cellAvailableSize.Height -= m_RowHeights[rowIndex];
					columnIndex = 0;
					rowIndex++;
				}
			}

			m_TotalAutoFixedSize = Size.Zero;

			int width = 0;
			for (columnIndex = 0; columnIndex < m_ColumnCount; columnIndex++)
			{
				if (m_RequestedColumnWidths != null)
				{
					float w = m_RequestedColumnWidths[columnIndex];
					if (w > 1.0f)
					{
						if (m_ColumnWidths[columnIndex] < w)
							m_ColumnWidths[columnIndex] = (int)w;

						m_TotalAutoFixedSize.Width += m_ColumnWidths[columnIndex];
					}
					else if (Single.IsNaN(w))
					{
						m_TotalAutoFixedSize.Width += m_ColumnWidths[columnIndex];
					}
				}
				else
				{
					m_TotalAutoFixedSize.Width += m_ColumnWidths[columnIndex];
				}

				width += m_ColumnWidths[columnIndex];
			}

			int height = 0;
			for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				if (m_RequestedRowHeights != null)
				{
					float h = m_RequestedRowHeights[rowIndex];
					if (h > 1.0f)
					{
						if (m_RowHeights[rowIndex] < h)
							m_RowHeights[rowIndex] = (int)h;

						m_TotalAutoFixedSize.Height += m_RowHeights[rowIndex];
					}
					else if (Single.IsNaN(h))
					{
						m_TotalAutoFixedSize.Height += m_RowHeights[rowIndex];
					}
				}
				else
				{
					m_TotalAutoFixedSize.Height += m_RowHeights[rowIndex];
				}

				height += m_RowHeights[rowIndex];
			}

			return new Size(width, height) + Padding;
		}

		protected override Size OnArrange(Size finalSize)
		{
			int y = Padding.Top;
			int x = Padding.Left;
			int columnIndex = 0;
			int rowIndex = 0;

			foreach (ControlBase child in this.Children)
			{
				int width = m_ColumnWidths[columnIndex];
				int height = m_RowHeights[rowIndex];

				if (!child.IsCollapsed)
				{
					if (m_RequestedColumnWidths != null)
					{
						float w = m_RequestedColumnWidths[columnIndex];
						if (w >= 0.0f && w <= 1.0f)
							width = Math.Max(0, (int)(w * (finalSize.Width - m_TotalAutoFixedSize.Width)));
						else if (w > 1.0f)
							width = (int)w;
					}
					if (m_RequestedRowHeights != null)
					{
						float h = m_RequestedRowHeights[rowIndex];
						if (h >= 0.0f && h <= 1.0f)
							height = Math.Max(0, (int)(h * (finalSize.Height - m_TotalAutoFixedSize.Height)));
						else if (h > 1.0f)
							height = (int)h;
					}

					child.Arrange(new Rectangle(x, y, width, height));
				}

				x += width;
				columnIndex++;
				if (columnIndex == m_ColumnCount)
				{
					x = Padding.Left;
					y += height;
					columnIndex = 0;
					rowIndex++;
				}
			}

			return finalSize;
		}
	}
}
