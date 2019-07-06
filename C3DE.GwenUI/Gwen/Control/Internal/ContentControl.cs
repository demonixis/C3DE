using System;
using System.Collections.Generic;

namespace Gwen.Control.Internal
{
	public class ContentControl : ControlBase
	{
		/// <summary>
		/// If the innerpanel exists our children will automatically become children of that instead of us.
		/// </summary>
		protected ControlBase m_InnerPanel;

		/// <summary>
		/// Logical list of children. If InnerPanel is not null, returns InnerPanel's children.
		/// </summary>
		public override List<ControlBase> Children
		{
			get
			{
				if (m_InnerPanel != null)
					return m_InnerPanel.Children;
				return base.Children;
			}
		}

		/// <summary>
		/// Get the content of the control.
		/// </summary>
		public virtual ControlBase Content
		{
			get { return m_InnerPanel; }
		}

		public ContentControl(ControlBase parent)
			: base(parent)
		{

		}

		/// <summary>
		/// Attaches specified control as a child of this one.
		/// </summary>
		/// <remarks>
		/// If InnerPanel is not null, it will become the parent.
		/// </remarks>
		/// <param name="child">Control to be added as a child.</param>
		public override void AddChild(ControlBase child)
		{
			if (m_InnerPanel != null)
			{
				m_InnerPanel.AddChild(child);
			}
			else
			{
				base.AddChild(child);
			}

			OnChildAdded(child);
		}

		/// <summary>
		/// Detaches specified control from this one.
		/// </summary>
		/// <param name="child">Child to be removed.</param>
		/// <param name="dispose">Determines whether the child should be disposed (added to delayed delete queue).</param>
		public override void RemoveChild(ControlBase child, bool dispose)
		{
			// If we removed our innerpanel
			// remove our pointer to it
			if (m_InnerPanel == child)
			{
				base.RemoveChild(child, dispose);
				m_InnerPanel = null;
				return;
			}

			if (m_InnerPanel != null && m_InnerPanel.Children.Contains(child))
			{
				m_InnerPanel.RemoveChild(child, dispose);
				return;
			}

			base.RemoveChild(child, dispose);
		}

		/// <summary>
		/// Finds a child by name.
		/// </summary>
		/// <param name="name">Child name.</param>
		/// <param name="recursive">Determines whether the search should be recursive.</param>
		/// <returns>Found control or null.</returns>
		public override ControlBase FindChildByName(string name, bool recursive = false)
		{
			if (m_InnerPanel != null && m_InnerPanel is InnerContentControl)
				return m_InnerPanel.FindChildByName(name, recursive);

			return base.FindChildByName(name, recursive);
		}
	}
}
