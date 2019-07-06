using System;
using System.Collections.Generic;

namespace Gwen.Control
{
	/// <summary>
	/// Tree control.
	/// </summary>
	[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
	public class TreeControl : ScrollControl
	{
		private readonly TreeNode m_RootNode;
		private bool m_MultiSelect;

		/// <summary>
		/// List of selected nodes.
		/// </summary>
		public IEnumerable<TreeNode> SelectedNodes
		{
			get
			{
				List<TreeNode> selectedNodes = new List<TreeNode>();

				foreach (ControlBase child in m_RootNode.Children)
				{
					TreeNode node = child as TreeNode;
					if (node == null)
						continue;
					selectedNodes.AddRange(node.SelectedChildren);
				}

				return selectedNodes;
			}
		}

		/// <summary>
		/// First selected node (and only if nodes are not multiselectable).
		/// </summary>
		public TreeNode SelectedNode
		{
			get
			{
				List<TreeNode> selectedNodes = SelectedNodes as List<TreeNode>;
				if (selectedNodes.Count > 0)
					return selectedNodes[0];
				else
					return null;
			}
		}

		/// <summary>
		/// Determines if multiple nodes can be selected at the same time.
		/// </summary>
		[Xml.XmlProperty]
		public bool AllowMultiSelect { get { return m_MultiSelect; } set { m_MultiSelect = value; } }

		/// <summary>
		/// Get the root node of the tree view. Root node is an invisible always expanded node that works
		/// as a parent node for all first tier nodes visible on the control.
		/// </summary>
		public TreeNode RootNode { get { return m_RootNode; } }

		/// <summary>
		/// Invoked when the node's selected state has changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> SelectionChanged
		{
			add
			{
				m_RootNode.SelectionChanged += value;
			}
			remove
			{
				m_RootNode.SelectionChanged -= value;
			}
		}

		/// <summary>
		/// Invoked when the node has been selected.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Selected
		{
			add
			{
				m_RootNode.Selected += value;
			}
			remove
			{
				m_RootNode.Selected -= value;
			}
		}

		/// <summary>
		/// Invoked when the node has been unselected.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Unselected
		{
			add
			{
				m_RootNode.Unselected += value;
			}
			remove
			{
				m_RootNode.Unselected -= value;
			}
		}

		/// <summary>
		/// Invoked when the node has been double clicked and contains no child nodes.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> NodeDoubleClicked
		{
			add
			{
				m_RootNode.NodeDoubleClicked += value;
			}
			remove
			{
				m_RootNode.NodeDoubleClicked -= value;
			}
		}

		/// <summary>
		/// Invoked when the node has been expanded.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Expanded
		{
			add
			{
				m_RootNode.Expanded += value;
			}
			remove
			{
				m_RootNode.Expanded -= value;
			}
		}

		/// <summary>
		/// Invoked when the node has been collapsed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Collapsed
		{
			add
			{
				m_RootNode.Collapsed += value;
			}
			remove
			{
				m_RootNode.Collapsed -= value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TreeControl"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public TreeControl(ControlBase parent)
			: base(parent)
		{
			Padding = Padding.One;

			MouseInputEnabled = true;
			EnableScroll(true, true);
			AutoHideBars = true;

			m_MultiSelect = false;

			m_RootNode = new TreeNode(this);
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			if (ShouldDrawBackground)
				skin.DrawTreeControl(this);
		}

		/// <summary>
		/// Adds a new child node.
		/// </summary>
		/// <param name="label">Node's label.</param>
		/// <returns>Newly created control.</returns>
		public TreeNode AddNode(string label, string name = null, object userData = null)
		{
			return m_RootNode.AddNode(label, name, userData);
		}

		/// <summary>
		/// Removes all child nodes.
		/// </summary>
		public virtual void RemoveAll()
		{
			m_RootNode.DeleteAllChildren();
		}

		/// <summary>
		/// Remove node and all of it's child nodes.
		/// </summary>
		/// <param name="node">Node to remove.</param>
		public void RemoveNode(TreeNode node)
		{
			if (node == null)
				return;

			m_RootNode.RemoveNode(node);
		}

		/// <summary>
		/// Remove all nodes.
		/// </summary>
		public void RemoveAllNodes()
		{
			m_RootNode.RemoveAllNodes();
		}

		/// <summary>
		/// Opens the node and all child nodes.
		/// </summary>
		public void ExpandAll()
		{
			m_RootNode.ExpandAll();
		}

		/// <summary>
		/// Clears the selection on the node and all child nodes.
		/// </summary>
		public void UnselectAll()
		{
			m_RootNode.UnselectAll();
		}

		/// <summary>
		/// Find a node bu user data.
		/// </summary>
		/// <param name="userData">Node user data.</param>
		/// <param name="recursive">Determines whether the search should be recursive.</param>
		/// <returns>Found node or null.</returns>
		public TreeNode FindNodeByUserData(object userData, bool recursive = true)
		{
			return m_RootNode.FindNodeByUserData(userData, recursive);
		}

		/// <summary>
		/// Find a node by name.
		/// </summary>
		/// <param name="name">Node name</param>
		/// <param name="recursive">Determines whether the search should be recursive.</param>
		/// <returns>Found node or null.</returns>
		public TreeNode FindNodeByName(string name, bool recursive = true)
		{
			return m_RootNode.FindNodeByName(name, recursive) as TreeNode;
		}

		/// <summary>
		/// Handler for node added event.
		/// </summary>
		/// <param name="node">Node added.</param>
		public virtual void OnNodeAdded(TreeNode node)
		{
			node.LabelPressed += OnNodeSelected;
		}

		/// <summary>
		/// Handler for node selected event.
		/// </summary>
		/// <param name="Control">Node selected.</param>
		protected virtual void OnNodeSelected(ControlBase Control, EventArgs args)
		{
			if (!m_MultiSelect /*|| InputHandler.InputHandler.IsKeyDown(Key.Control)*/)
				UnselectAll();
		}

		internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent)
		{
			TreeControl element = new TreeControl(parent);
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
