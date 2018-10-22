using System;
using System.Linq;
using Gwen.Control.Internal;
using System.Collections.Generic;

namespace Gwen.Control
{
	/// <summary>
	/// Tree control node.
	/// </summary>
	[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
	public class TreeNode : ContentControl
	{
		private TreeControl m_TreeControl = null;
		protected TreeToggleButton m_ToggleButton = null;
		protected TreeNodeLabel m_Title = null;
		private bool m_Root = false;

		private bool m_Selected;
		private bool m_Selectable;

		/// <summary>
		/// Root node of the tree view.
		/// </summary>
		private TreeNode RootNode { get { return m_TreeControl.RootNode; } }

		/// <summary>
		/// Parent tree control.
		/// </summary>
		public TreeControl TreeControl { get { return m_TreeControl; } }

		/// <summary>
		/// Indicates whether this is a root node.
		/// </summary>
		public bool IsRoot { get { return m_Root; } }

		/// <summary>
		/// Determines whether the node is selectable.
		/// </summary>
		[Xml.XmlProperty]
		public bool IsSelectable { get { return m_Selectable; } set { m_Selectable = value; } }

		public int NodeCount { get { return Children.Count; } }

		/// <summary>
		/// Indicates whether the node is selected.
		/// </summary>
		[Xml.XmlProperty]
		public bool IsSelected
		{
			get { return m_Selected; }
			set
			{
				if (!IsSelectable)
					return;
				if (IsSelected == value)
					return;

				if (value && !TreeControl.AllowMultiSelect)
					RootNode.UnselectAll();

				m_Selected = value;

				if (m_Title != null)
					m_Title.ToggleState = value;

				if (SelectionChanged != null)
					SelectionChanged.Invoke(this, EventArgs.Empty);

				// propagate to root parent (tree)
				if (RootNode != null && RootNode.SelectionChanged != null)
					RootNode.SelectionChanged.Invoke(this, EventArgs.Empty);

				if (value)
				{
					if (Selected != null)
						Selected.Invoke(this, EventArgs.Empty);

					if (RootNode != null && RootNode.Selected != null)
						RootNode.Selected.Invoke(this, EventArgs.Empty);
				}
				else
				{
					if (Unselected != null)
						Unselected.Invoke(this, EventArgs.Empty);

					if (RootNode != null && RootNode.Unselected != null)
						RootNode.Unselected.Invoke(this, EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Node's label.
		/// </summary>
		[Xml.XmlProperty]
		public string Text { get { return m_Title.Text; } set { m_Title.Text = value; } }

		/// <summary>
		/// List of selected nodes.
		/// </summary>
		public IEnumerable<TreeNode> SelectedChildren
		{
			get
			{
				List<TreeNode> Trees = new List<TreeNode>();

				foreach (ControlBase child in Children)
				{
					TreeNode node = child as TreeNode;
					if (node == null)
						continue;
					Trees.AddRange(node.SelectedChildren);
				}

				if (this.IsSelected)
				{
					Trees.Add(this);
				}

				return Trees;
			}
		}

		/// <summary>
		/// Returns the current image name (or null if no image set) or set a new image.
		/// </summary>
		[Xml.XmlProperty]
		public string ImageName
		{
			get
			{
				if (m_Title != null)
					return m_Title.ImageName;
				else
					return null;
			}
			set
			{
				if (m_Title != null && m_Title.ImageName == value)
					return;

				SetImage(value);
			}
		}

		/// <summary>
		/// Invoked when the node label has been pressed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> LabelPressed;

		/// <summary>
		/// Invoked when the node's selected state has changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> SelectionChanged;

		/// <summary>
		/// Invoked when the node has been selected.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Selected;

		/// <summary>
		/// Invoked when the node has been double clicked and contains no child nodes.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> NodeDoubleClicked;

		/// <summary>
		/// Invoked when the node has been unselected.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Unselected;

		/// <summary>
		/// Invoked when the node has been expanded.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Expanded;

		/// <summary>
		/// Invoked when the node has been collapsed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Collapsed;

		/// <summary>
		/// Initializes a new instance of the <see cref="TreeNode"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public TreeNode(ControlBase parent)
			: base(parent)
		{
			// Make sure that the tree control has only one root node.
			if (m_TreeControl == null && parent is TreeControl)
			{
				m_TreeControl = parent as TreeControl;
				m_Root = true;
			}
			else
			{
				m_ToggleButton = new TreeToggleButton(this);
				m_ToggleButton.Toggled += OnToggleButtonPress;

				m_Title = new TreeNodeLabel(this);
				m_Title.DoubleClicked += OnDoubleClickName;
				m_Title.Clicked += OnClickName;
			}

			m_InnerPanel = new Layout.VerticalLayout(this);
			m_InnerPanel.Collapse(!m_Root, false); // Root node is always expanded

			m_Selected = false;
			m_Selectable = true;
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			if (!m_Root)
			{
				int bottom = 0;
				if (m_InnerPanel.Children.Count > 0)
				{
					bottom = m_InnerPanel.Children.Last().ActualTop + m_InnerPanel.ActualTop;
				}

				skin.DrawTreeNode(this, m_InnerPanel.IsVisible, IsSelected, m_Title.ActualHeight, m_Title.ActualWidth, (int)(m_ToggleButton.ActualTop + m_ToggleButton.ActualHeight * 0.5f), bottom, RootNode == Parent, m_ToggleButton.ActualWidth);
			}
		}

		protected override Size OnMeasure(Size availableSize)
		{
			if (!m_Root)
			{
				Size buttonSize = m_ToggleButton.Measure(availableSize);
				Size labelSize = m_Title.Measure(availableSize);
				Size innerSize = Size.Zero;

				if (m_InnerPanel.Children.Count == 0)
				{
					m_ToggleButton.Hide();
					m_ToggleButton.ToggleState = false;
					m_InnerPanel.Collapse(true, false);
				}
				else
				{
					m_ToggleButton.Show();
					if (!m_InnerPanel.IsCollapsed)
						innerSize = m_InnerPanel.Measure(availableSize);
				}

				return new Size(Math.Max(buttonSize.Width + labelSize.Width, m_ToggleButton.MeasuredSize.Width + innerSize.Width), Math.Max(buttonSize.Height, labelSize.Height) + innerSize.Height) + Padding;
			}
			else
			{
				return m_InnerPanel.Measure(availableSize) + Padding;
			}
		}

		protected override Size OnArrange(Size finalSize)
		{
			if (!m_Root)
			{
				m_ToggleButton.Arrange(new Rectangle(Padding.Left, Padding.Top + (m_Title.MeasuredSize.Height - m_ToggleButton.MeasuredSize.Height) / 2, m_ToggleButton.MeasuredSize.Width, m_ToggleButton.MeasuredSize.Height));
				m_Title.Arrange(new Rectangle(Padding.Left + m_ToggleButton.MeasuredSize.Width, Padding.Top, m_Title.MeasuredSize.Width, m_Title.MeasuredSize.Height));

				if (!m_InnerPanel.IsCollapsed)
					m_InnerPanel.Arrange(new Rectangle(Padding.Left + m_ToggleButton.MeasuredSize.Width, Padding.Top + Math.Max(m_ToggleButton.MeasuredSize.Height, m_Title.MeasuredSize.Height), m_InnerPanel.MeasuredSize.Width, m_InnerPanel.MeasuredSize.Height));
			}
			else
			{
				m_InnerPanel.Arrange(new Rectangle(Padding.Left, Padding.Top, m_InnerPanel.MeasuredSize.Width, m_InnerPanel.MeasuredSize.Height));
			}

			return MeasuredSize;
		}

		/// <summary>
		/// Adds a new child node.
		/// </summary>
		/// <param name="label">Node's label.</param>
		/// <returns>Newly created control.</returns>
		public TreeNode AddNode(string label, string name = null, object userData = null)
		{
			TreeNode node = new TreeNode(this);
			node.Text = label;
			node.Name = name;
			node.UserData = userData;

			return node;
		}

		public TreeNode InsertNode(int index, string label, string name = null, object userData = null)
		{
			TreeNode node = AddNode(label, name, userData);
			if (index == 0)
				node.SendToBack();
			else if (index < Children.Count)
				node.BringNextToControl(Children[index], false);

			return node;
		}

		/// <summary>
		/// Remove node and all of it's child nodes.
		/// </summary>
		/// <param name="node">Node to remove.</param>
		public void RemoveNode(TreeNode node)
		{
			if (node == null)
				return;

			node.RemoveAllNodes();

			RemoveChild(node, true);

			Invalidate();
		}

		/// <summary>
		/// Remove all nodes.
		/// </summary>
		public void RemoveAllNodes()
		{
			while (NodeCount > 0)
			{
				TreeNode node = Children[0] as TreeNode;
				if (node == null)
					continue;

				RemoveNode(node);
			}

			Invalidate();
		}

		/// <summary>
		/// Opens the node.
		/// </summary>
		public void Open()
		{
			m_InnerPanel.Show();
			if (m_ToggleButton != null)
				m_ToggleButton.ToggleState = true;

			if (Expanded != null)
				Expanded.Invoke(this, EventArgs.Empty);
			if (RootNode != null && RootNode.Expanded != null)
				RootNode.Expanded.Invoke(this, EventArgs.Empty);

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

			if (Collapsed != null)
				Collapsed.Invoke(this, EventArgs.Empty);
			if (RootNode != null && RootNode.Collapsed != null)
				RootNode.Collapsed.Invoke(this, EventArgs.Empty);

			Invalidate();
		}

		/// <summary>
		/// Opens the node and all child nodes.
		/// </summary>
		public void ExpandAll()
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
		/// Clears the selection on the node and all child nodes.
		/// </summary>
		public void UnselectAll()
		{
			IsSelected = false;
			if (m_Title != null)
				m_Title.ToggleState = false;

			foreach (ControlBase child in Children)
			{
				TreeNode node = child as TreeNode;
				if (node == null)
					continue;
				node.UnselectAll();
			}
		}

		/// <summary>
		/// Find a node bu user data.
		/// </summary>
		/// <param name="userData">Node user data.</param>
		/// <param name="recursive">Determines whether the search should be recursive.</param>
		/// <returns>Found node or null.</returns>
		public TreeNode FindNodeByUserData(object userData, bool recursive = true)
		{
			TreeNode node = this.Children.Where(x => x is TreeNode && x.UserData == userData).FirstOrDefault() as TreeNode;
			if (node != null)
				return node;

			if (recursive)
			{
				foreach (ControlBase child in this.Children)
				{
					node = child as TreeNode;
					if (node != null)
					{
						node = node.FindNodeByUserData(userData, true);
						if (node != null)
							return node;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Find a node by name.
		/// </summary>
		/// <param name="name">Node name</param>
		/// <param name="recursive">Determines whether the search should be recursive.</param>
		/// <returns>Found node or null.</returns>
		public TreeNode FindNodeByName(string name, bool recursive = true)
		{
			return FindChildByName(name, recursive) as TreeNode;
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
			{
				// Invoke double click events only if node hasn't child nodes.
				// Otherwise toggle expand/collapse.
				if (NodeDoubleClicked != null)
					NodeDoubleClicked.Invoke(this, EventArgs.Empty);

				if (RootNode != null && RootNode.NodeDoubleClicked != null)
					RootNode.NodeDoubleClicked.Invoke(this, EventArgs.Empty);

				return;
			}

			m_ToggleButton.Toggle();
		}

		/// <summary>
		/// Handler for label click.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected virtual void OnClickName(ControlBase control, EventArgs args)
		{
			if (LabelPressed != null)
				LabelPressed.Invoke(this, EventArgs.Empty);
			IsSelected = !IsSelected;
		}

		/// <summary>
		/// Set tree node image.
		/// </summary>
		/// <param name="textureName">Image name.</param>
		public void SetImage(string textureName) 
		{
			m_Title.SetImage(textureName);
		}

		protected override void OnChildAdded(ControlBase child)
		{
			TreeNode node = child as TreeNode;
			if (node != null)
			{
				node.m_TreeControl = m_TreeControl;

				m_TreeControl.OnNodeAdded(node);
			}

			base.OnChildAdded(child);
		}

		[Xml.XmlEvent]
		public override event GwenEventHandler<ClickedEventArgs> Clicked
		{ 
			add
			{
				m_Title.Clicked += delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
			remove
			{
				m_Title.Clicked -= delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
		}

		[Xml.XmlEvent]
		public override event GwenEventHandler<ClickedEventArgs> DoubleClicked 
		{ 
			add
			{
				if (value != null)
				{
					m_Title.DoubleClicked += delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
				}
			}
			remove
			{
				m_Title.DoubleClicked -= delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
		}

		[Xml.XmlEvent]
		public override event GwenEventHandler<ClickedEventArgs> RightClicked {
			add
			{
				m_Title.RightClicked += delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
			remove
			{
				m_Title.RightClicked -= delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
		}

		[Xml.XmlEvent]
		public override event GwenEventHandler<ClickedEventArgs> DoubleRightClicked {
			add
			{
				if (value != null)
				{
					m_Title.DoubleRightClicked += delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
				}
			}
			remove
			{
				m_Title.DoubleRightClicked -= delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
		}

		internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent)
		{
			TreeNode element = new TreeNode(parent);
			parser.ParseAttributes(element);
			if (parser.MoveToContent())
			{
				foreach (string elementName in parser.NextElement())
				{
					if (elementName == "TreeNode")
					{
						parser.ParseElement<TreeNode>(element);
					}
				}
			}

			return element;
		}
	}
}
