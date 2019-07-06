using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Properties node.
    /// </summary>
    public class PropertyTreeNode : ContentControl
    {
		public const int TreeIndentation = 14;

		protected readonly PropertyTree m_PropertyTree;
		protected readonly TreeToggleButton m_ToggleButton;
		protected readonly TreeNodeLabel m_Title;
		protected readonly Properties m_Properties;

		public PropertyTree PropertyTree { get { return m_PropertyTree; } }

		public Properties Properties { get { return m_Properties; } }

		/// <summary>
		/// Node's label.
		/// </summary>
		public string Text { get { return m_Title.Text; } set { m_Title.Text = value; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyTreeNode"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public PropertyTreeNode(ControlBase parent)
            : base(parent)
        {
			m_PropertyTree = parent as PropertyTree;

			m_ToggleButton = new TreeToggleButton(this);
			m_ToggleButton.Toggled += OnToggleButtonPress;

			m_Title = new TreeNodeLabel(this);
			m_Title.DoubleClicked += OnDoubleClickName;

			m_Properties = new Properties(this);

			m_InnerPanel = m_Properties;

			m_Title.TextColorOverride = Skin.Colors.Properties.Title;
        }

		protected override Size OnMeasure(Size availableSize)
		{
			Size buttonSize = m_ToggleButton.Measure(availableSize);
			Size labelSize = m_Title.Measure(availableSize);
			Size innerSize = Size.Zero;

			if (!m_InnerPanel.IsCollapsed)
				innerSize = m_InnerPanel.Measure(availableSize);

			return new Size(Math.Max(buttonSize.Width + labelSize.Width, TreeIndentation + innerSize.Width), Math.Max(buttonSize.Height, labelSize.Height) + innerSize.Height);
		}

		protected override Size OnArrange(Size finalSize)
		{
			m_ToggleButton.Arrange(new Rectangle(0, (m_Title.MeasuredSize.Height - m_ToggleButton.MeasuredSize.Height) / 2, m_ToggleButton.MeasuredSize.Width, m_ToggleButton.MeasuredSize.Height));
			m_Title.Arrange(new Rectangle(m_ToggleButton.MeasuredSize.Width, 0, finalSize.Width - m_ToggleButton.MeasuredSize.Width, m_Title.MeasuredSize.Height));

			if (!m_InnerPanel.IsCollapsed)
				m_InnerPanel.Arrange(new Rectangle(TreeIndentation, Math.Max(m_ToggleButton.MeasuredSize.Height, m_Title.MeasuredSize.Height), finalSize.Width - TreeIndentation, m_InnerPanel.MeasuredSize.Height));

			return new Size(finalSize.Width, MeasuredSize.Height);
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawPropertyTreeNode(this, m_InnerPanel.ActualLeft, m_InnerPanel.ActualTop);
        }

		/// <summary>
		/// Opens the node.
		/// </summary>
		public void Open()
		{
			m_InnerPanel.Show();
			if (m_ToggleButton != null)
				m_ToggleButton.ToggleState = true;

			Invalidate();
		}

		/// <summary>
		/// Closes the node.
		/// </summary>
		public void Close()
		{
			m_InnerPanel.Collapse();
			if (m_ToggleButton != null)
				m_ToggleButton.ToggleState = false;

			Invalidate();
		}

		/// <summary>
		/// Opens the node and all child nodes.
		/// </summary>
		public void Expand()
		{
			Open();
			foreach (ControlBase child in Children)
			{
				TreeNode node = child as TreeNode;
				if (node == null)
					continue;
				node.ExpandAll();
			}
		}

		/// <summary>
		/// Handler for the toggle button.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected virtual void OnToggleButtonPress(ControlBase control, EventArgs args)
		{
			if (m_ToggleButton.ToggleState)
			{
				Open();
			}
			else
			{
				Close();
			}
		}

		/// <summary>
		/// Handler for label double click.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected virtual void OnDoubleClickName(ControlBase control, EventArgs args)
		{
			if (!m_ToggleButton.IsVisible)
				return;
			m_ToggleButton.Toggle();
		}
	}
}
