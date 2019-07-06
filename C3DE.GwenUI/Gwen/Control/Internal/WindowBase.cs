using System;
using System.Linq;

namespace Gwen.Control
{
	public enum StartPosition
	{
		CenterParent,
		CenterCanvas,
		Manual
	}
}

namespace Gwen.Control.Internal
{
	public abstract class WindowBase : ResizableControl
	{
		private bool m_DeleteOnClose;
		private ControlBase m_RealParent;
		private StartPosition m_StartPosition = StartPosition.Manual;

		protected Dragger m_DragBar;

		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Closed;

		/// <summary>
		/// Is window draggable.
		/// </summary>
		[Xml.XmlProperty]
		public bool IsDraggingEnabled { get { return m_DragBar.Target != null; } set { m_DragBar.Target = value ? this : null; } }

		/// <summary>
		/// Determines whether the control should be disposed on close.
		/// </summary>
		[Xml.XmlProperty]
		public bool DeleteOnClose { get { return m_DeleteOnClose; } set { m_DeleteOnClose = value; } }

		[Xml.XmlProperty]
		public override Padding Padding { get { return m_InnerPanel.Padding; } set { m_InnerPanel.Padding = value; } }

		/// <summary>
		/// Starting position of the window.
		/// </summary>
		[Xml.XmlProperty]
		public StartPosition StartPosition { get { return m_StartPosition; } set { m_StartPosition = value; } }

		/// <summary>
		/// Indicates whether the control is on top of its parent's children.
		/// </summary>
		public override bool IsOnTop
		{
			get { return Parent.Children.Where(x => x is WindowBase).Last() == this; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WindowBase"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public WindowBase(ControlBase parent)
			: base(parent.GetCanvas())
		{
			m_RealParent = parent;

			EnableResizing();
			BringToFront();
			IsTabable = false;
			Focus();
			MinimumSize = new Size(100, 40);
			ClampMovement = true;
			KeyboardInputEnabled = false;
			MouseInputEnabled = true;
		}

		public override void Show()
		{
			BringToFront();
			base.Show();
		}

		public virtual void Close()
		{
			IsCollapsed = true;

			if (m_DeleteOnClose)
			{
				Parent.RemoveChild(this, true);
			}

			if (Closed != null)
				Closed(this, EventArgs.Empty);
		}

		public override void Touch()
		{
			base.Touch();
			BringToFront();
		}

		protected virtual void OnDragged(ControlBase control, EventArgs args)
		{
			m_StartPosition = StartPosition.Manual;
		}

		protected override void OnResized(ControlBase control, EventArgs args)
		{
			m_StartPosition = StartPosition.Manual;

			base.OnResized(control, args);
		}

		public override bool SetBounds(int x, int y, int width, int height)
		{
			if (m_StartPosition == StartPosition.CenterCanvas)
			{
				ControlBase canvas = GetCanvas();
				x = (canvas.ActualWidth - width) / 2;
				y = (canvas.ActualHeight - height) / 2;
			}
			else if (m_StartPosition == StartPosition.CenterParent)
			{
				Point pt = m_RealParent.LocalPosToCanvas(new Point(m_RealParent.ActualWidth / 2, m_RealParent.ActualHeight / 2));
				x = pt.X - width / 2;
				y = pt.Y - height / 2;
			}

			return base.SetBounds(x, y, width, height);
		}
	}
}
