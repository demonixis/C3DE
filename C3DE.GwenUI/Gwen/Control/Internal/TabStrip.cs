using System;
using Gwen.Control.Internal;
using Gwen.DragDrop;

namespace Gwen.Control.Internal
{
	/// <summary>
	/// Tab strip - groups TabButtons and allows reordering.
	/// </summary>
	public class TabStrip : Layout.StackLayout
	{
		private ControlBase m_TabDragControl;
		private bool m_AllowReorder;
		private int m_ScrollOffset;
		private Size m_TotalSize;

		/// <summary>
		/// Determines whether it is possible to reorder tabs by mouse dragging.
		/// </summary>
		public bool AllowReorder { get { return m_AllowReorder; } set { m_AllowReorder = value; } }

		internal int ScrollOffset
		{
			get { return m_ScrollOffset; }
			set { SetScrollOffset(value); }
		}

		internal Size TotalSize { get { return m_TotalSize; } }

		/// <summary>
		/// Determines whether the control should be clipped to its bounds while rendering.
		/// </summary>
		protected override bool ShouldClip
		{
			get { return false; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TabStrip"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public TabStrip(ControlBase parent)
			: base(parent)
		{
			m_AllowReorder = false;
			m_ScrollOffset = 0;
		}

		/// <summary>
		/// Strip position (top/left/right/bottom).
		/// </summary>
		public Dock StripPosition
		{
			get { return Dock; }
			set
			{
				Dock = value;

				switch (value)
				{
					case Dock.Top:
						Padding = new Padding(5, 0, 0, 0);
						Horizontal = true;
						break;
					case Dock.Left:
						Padding = new Padding(0, 5, 0, 0);
						Horizontal = false;
						break;
					case Dock.Bottom:
						Padding = new Padding(5, 0, 0, 0);
						Horizontal = true;
						break;
					case Dock.Right:
						Padding = new Padding(0, 5, 0, 0);
						Horizontal = false;
						break;
				}
			}
		}

		private void SetScrollOffset(int value)
		{
			for (int i = 0; i < Children.Count; i++)
			{
				if (i < value && i < (Children.Count - 1))
					Children[i].Collapse(true, false);
				else
					Children[i].Collapse(false, false);
			}

			m_ScrollOffset = value;
			m_ScrollOffset = Math.Min(m_ScrollOffset, Children.Count - 1);
			m_ScrollOffset = Math.Max(m_ScrollOffset, 0);

			Invalidate();
		}

		protected override Size OnMeasure(Size availableSize)
		{
			int num = 0;
			foreach (var child in Children)
			{
				TabButton button = child as TabButton;
				if (null == button) continue;

				Margin m = new Margin();
				int notFirst = num > 0 ? -1 : 0;

				switch (this.StripPosition)
				{
					case Dock.Top:
					case Dock.Bottom:
						m.Left = notFirst;
						break;
					case Dock.Left:
					case Dock.Right:
						m.Top = notFirst;
						break;
				}

				button.Margin = m;
				num++;
			}

			m_TotalSize = base.OnMeasure(Size.Infinity);

			return m_TotalSize;
		}

		public override void DragAndDrop_HoverEnter(Package p, int x, int y)
		{
			if (m_TabDragControl != null)
			{
				throw new InvalidOperationException("ERROR! TabStrip::DragAndDrop_HoverEnter");
			}

			m_TabDragControl = new Highlight(GetCanvas());
			m_TabDragControl.MouseInputEnabled = false;
			m_TabDragControl.Size = new Size(3, ActualHeight);
			Invalidate();
		}

		public override void DragAndDrop_HoverLeave(Package p)
		{
			if (m_TabDragControl != null)
			{
				m_TabDragControl.Parent.RemoveChild(m_TabDragControl, false); // [omeg] need to do that explicitely
				m_TabDragControl.Dispose();
			}
			m_TabDragControl = null;
		}

		public override void DragAndDrop_Hover(Package p, int x, int y)
		{
			Point localPos = CanvasPosToLocal(new Point(x, y));

			ControlBase droppedOn = GetControlAt(localPos.X, localPos.Y);
			if (droppedOn != null && droppedOn != this)
			{
				Point dropPos = droppedOn.CanvasPosToLocal(new Point(x, y));
				m_TabDragControl.BringToFront();
				int pos = droppedOn.ActualLeft - 1;

				if (dropPos.X > droppedOn.ActualWidth/2)
					pos += droppedOn.ActualWidth - 1;

				Point canvasPos = LocalPosToCanvas(new Point(pos, 0));
				m_TabDragControl.MoveTo(canvasPos.X, canvasPos.Y);
			}
			else
			{
				m_TabDragControl.BringToFront();
			}
		}

		public override bool DragAndDrop_HandleDrop(Package p, int x, int y)
		{
			Point LocalPos = CanvasPosToLocal(new Point(x, y));

			TabButton button = DragAndDrop.SourceControl as TabButton;
			TabControl tabControl = Parent as TabControl;
			if (tabControl != null && button != null)
			{
				if (button.TabControl != tabControl)
				{
					// We've moved tab controls!
					tabControl.AddPage(button);
				}
			}

			ControlBase droppedOn = GetControlAt(LocalPos.X, LocalPos.Y);
			if (droppedOn != null && droppedOn != this)
			{
				Point dropPos = droppedOn.CanvasPosToLocal(new Point(x, y));
				DragAndDrop.SourceControl.BringNextToControl(droppedOn, dropPos.X > droppedOn.ActualWidth / 2);
			}
			else
			{
				DragAndDrop.SourceControl.BringToFront();
			}
			return true;
		}

		public override bool DragAndDrop_CanAcceptPackage(Package p)
		{
			if (!m_AllowReorder)
				return false;

			if (p.Name == "TabButtonMove")
				return true;

			return false;
		}
	}
}
