using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
    /// <summary>
    /// Properties table.
    /// </summary>
    public class Properties : ContentControl
    {
        private readonly SplitterBar m_SplitterBar;
		private int m_LabelWidth;

		internal const int DefaultLabelWidth = 80;

        /// <summary>
        /// Width of the first column (property names).
        /// </summary>
        internal int LabelWidth { get { return m_LabelWidth; } set { if (value == m_LabelWidth) return; m_LabelWidth = value; Invalidate(); } }

        /// <summary>
        /// Invoked when a property value has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Properties"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Properties(ControlBase parent)
            : base(parent)
        {
            m_SplitterBar = new SplitterBar(this);
			m_SplitterBar.Width = 3;
            m_SplitterBar.Cursor = Cursor.SizeWE;
            m_SplitterBar.Dragged += OnSplitterMoved;
            m_SplitterBar.ShouldDrawBackground = false;

			m_LabelWidth = DefaultLabelWidth;

			m_InnerPanel = new Layout.VerticalLayout(this);
		}

		protected override Size OnMeasure(Size availableSize)
		{
			availableSize -= Padding;

			Size size = m_InnerPanel.Measure(availableSize);

			m_SplitterBar.Measure(new Size(availableSize.Width, size.Height));

			return size + Padding;
		}

		protected override Size OnArrange(Size finalSize)
		{
			finalSize -= Padding;

			m_InnerPanel.Arrange(Padding.Left, Padding.Top, finalSize.Width, finalSize.Height);

			m_SplitterBar.Arrange(Padding.Left + m_LabelWidth - 2, Padding.Top, m_SplitterBar.MeasuredSize.Width, m_InnerPanel.MeasuredSize.Height);

			return new Size(finalSize.Width, m_InnerPanel.MeasuredSize.Height) + Padding;
		}

		/// <summary>
		/// Handles the splitter moved event.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected virtual void OnSplitterMoved(ControlBase control, EventArgs args)
        {
			LabelWidth = m_SplitterBar.ActualLeft - Padding.Left;

			PropertyTreeNode node = Parent as PropertyTreeNode;
			if (node != null)
			{
				node.PropertyTree.LabelWidth = LabelWidth;
			}
        }

        /// <summary>
        /// Adds a new text property row.
        /// </summary>
        /// <param name="label">Property name.</param>
        /// <param name="value">Initial value.</param>
        /// <returns>Newly created row.</returns>
        public PropertyRow Add(string label, string value = "")
        {
            return Add(label, new Property.Text(this), value);
        }

        /// <summary>
        /// Adds a new property row.
        /// </summary>
        /// <param name="label">Property name.</param>
        /// <param name="prop">Property control.</param>
        /// <param name="value">Initial value.</param>
        /// <returns>Newly created row.</returns>
        public PropertyRow Add(string label, Property.PropertyBase prop, string value = "")
        {
            PropertyRow row = new PropertyRow(this, prop);
            row.Label = label;
            row.ValueChanged += OnRowValueChanged;

            prop.SetValue(value, true);

            m_SplitterBar.BringToFront();
            return row;
        }

		private void OnRowValueChanged(ControlBase control, EventArgs args)
        {
            if (ValueChanged != null)
				ValueChanged.Invoke(control, EventArgs.Empty);
        }

        /// <summary>
        /// Deletes all rows.
        /// </summary>
        public void DeleteAll()
        {
            m_InnerPanel.DeleteAllChildren();
        }
    }
}
