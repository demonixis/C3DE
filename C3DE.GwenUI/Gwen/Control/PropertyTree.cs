using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
    /// <summary>
    /// Property table/tree.
    /// </summary>
    public class PropertyTree : ScrollControl
	{
		/// <summary>
		/// Width of the first column (property names).
		/// </summary>
		public int LabelWidth
		{
			get
			{
				foreach (ControlBase child in Children)
				{
					PropertyTreeNode node = child as PropertyTreeNode;
					if (node != null)
						return node.Properties.LabelWidth;
				}
				return Properties.DefaultLabelWidth;
			}
			set
			{
				foreach (ControlBase child in Children)
				{
					PropertyTreeNode node = child as PropertyTreeNode;
					if (node != null)
					{
						node.Properties.LabelWidth = value;
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyTree"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public PropertyTree(ControlBase parent)
			: base(parent)
		{
			Padding = Padding.One;

			MouseInputEnabled = true;
			EnableScroll(false, true);
			AutoHideBars = true;

			new Layout.VerticalLayout(this);
		}

		/// <summary>
		/// Adds a new properties node.
		/// </summary>
		/// <param name="label">Node label.</param>
		/// <returns>Newly created control</returns>
		public Properties Add(string label)
		{
			PropertyTreeNode node = new PropertyTreeNode(this);
			node.Text = label;

			return node.Properties;
		}

		/// <summary>
		/// Opens the node and all child nodes.
		/// </summary>
		public void ExpandAll()
		{
			foreach (ControlBase child in Children)
			{
				PropertyTreeNode node = child as PropertyTreeNode;
				if (node == null)
					continue;
				node.Open();
			}
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			skin.DrawCategoryHolder(this);
		}
    }
}
