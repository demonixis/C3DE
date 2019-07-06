using System;
using Gwen.Control.Internal;
using Gwen.DragDrop;

namespace Gwen.Control
{
    /// <summary>
    /// Base for dockable containers.
    /// </summary>
    public class DockControl : ControlBase
    {
        private DockControl m_Left;
        private DockControl m_Right;
        private DockControl m_Top;
        private DockControl m_Bottom;
        private Resizer m_Sizer;

        // Only CHILD dockpanels have a tabcontrol.
        private DockedTabControl m_DockedTabControl;

        private bool m_DrawHover;
        private bool m_DropFar;
        private Rectangle m_HoverRect;

        // todo: dock events?

        /// <summary>
        /// Control docked on the left side.
        /// </summary>
        public DockControl LeftDock { get { return GetChildDock(Dock.Left); } }

        /// <summary>
        /// Control docked on the right side.
        /// </summary>
        public DockControl RightDock { get { return GetChildDock(Dock.Right); } }

        /// <summary>
        /// Control docked on the top side.
        /// </summary>
        public DockControl TopDock { get { return GetChildDock(Dock.Top); } }

        /// <summary>
        /// Control docked on the bottom side.
        /// </summary>
        public DockControl BottomDock { get { return GetChildDock(Dock.Bottom); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public DockControl(ControlBase parent)
            : base(parent)
        {
            Padding = Padding.One;
			MinimumSize = new Size(30, 30);
			MouseInputEnabled = true;
        }

		/// <summary>
		/// Add a new dock control.
		/// </summary>
		/// <param name="title">Title visible on title bar or tab.</param>
		/// <param name="control">Control to add.</param>
		public void Add(string title, ControlBase control)
		{
			m_DockedTabControl.AddPage(title, control);
		}

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeySpace(bool down)
        {
            // No action on space (default button action is to press)
            return false;
        }

        /// <summary>
        /// Initializes an inner docked control for the specified position.
        /// </summary>
        /// <param name="pos">Dock position.</param>
        protected virtual void SetupChildDock(Dock pos)
        {
            if (m_DockedTabControl == null)
            {
                m_DockedTabControl = new DockedTabControl(this);
                m_DockedTabControl.TabRemoved += OnTabRemoved;
                m_DockedTabControl.TabStripPosition = Dock.Bottom;
                m_DockedTabControl.TitleBarVisible = true;
			}

            Dock = pos;

			Dock sizeDir;
            if (pos == Dock.Right) sizeDir = Dock.Left;
            else if (pos == Dock.Left) sizeDir = Dock.Right;
            else if (pos == Dock.Top) sizeDir = Dock.Bottom;
            else if (pos == Dock.Bottom) sizeDir = Dock.Top;
            else throw new ArgumentException("Invalid dock", "pos");

            if (m_Sizer != null)
                m_Sizer.Dispose();

            m_Sizer = new Resizer(this);
            m_Sizer.Dock = sizeDir;
            m_Sizer.ResizeDir = sizeDir;
			if (sizeDir == Dock.Left || sizeDir == Dock.Right)
				m_Sizer.Width = 2;
			else
				m_Sizer.Height = 2;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {

        }

        /// <summary>
        /// Gets an inner docked control for the specified position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected virtual DockControl GetChildDock(Dock pos)
        {
            // todo: verify
            DockControl dock = null;
            switch (pos)
            {
                case Dock.Left:
                    if (m_Left == null)
                    {
                        m_Left = new DockControl(this);
						m_Left.Width = 200;
                        m_Left.SetupChildDock(pos);
                    }
                    dock = m_Left;
                    break;

                case Dock.Right:
                    if (m_Right == null)
                    {
                        m_Right = new DockControl(this);
						m_Right.Width = 200;
						m_Right.SetupChildDock(pos);
                    }
                    dock = m_Right;
                    break;

                case Dock.Top:
                    if (m_Top == null)
                    {
                        m_Top = new DockControl(this);
						m_Top.Height = 200;
						m_Top.SetupChildDock(pos);
                    }
                    dock = m_Top;
                    break;

                case Dock.Bottom:
                    if (m_Bottom == null)
                    {
                        m_Bottom = new DockControl(this);
						m_Bottom.Height = 200;
						m_Bottom.SetupChildDock(pos);
                    }
                    dock = m_Bottom;
                    break;
            }

			if (dock != null)
				dock.Show();

            return dock;
        }

        /// <summary>
        /// Calculates dock direction from dragdrop coordinates.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>Dock direction.</returns>
        protected virtual Dock GetDroppedTabDirection(int x, int y)
        {
            int w = ActualWidth;
            int h = ActualHeight;
            float top = y / (float)h;
            float left = x / (float)w;
            float right = (w - x) / (float)w;
            float bottom = (h - y) / (float)h;
            float minimum = Math.Min(Math.Min(Math.Min(top, left), right), bottom);

            m_DropFar = (minimum < 0.2f);

            if (minimum > 0.3f)
                return Dock.Fill;

            if (top == minimum && (null == m_Top || m_Top.IsCollapsed))
                return Dock.Top;
            if (left == minimum && (null == m_Left || m_Left.IsCollapsed))
                return Dock.Left;
            if (right == minimum && (null == m_Right || m_Right.IsCollapsed))
                return Dock.Right;
            if (bottom == minimum && (null == m_Bottom || m_Bottom.IsCollapsed))
                return Dock.Bottom;

            return Dock.Fill;
        }

        public override bool DragAndDrop_CanAcceptPackage(Package p)
        {
            // A TAB button dropped 
            if (p.Name == "TabButtonMove")
                return true;

            // a TAB window dropped
            if (p.Name == "TabWindowMove")
                return true;

            return false;
        }

        public override bool DragAndDrop_HandleDrop(Package p, int x, int y)
        {
            Point pos = CanvasPosToLocal(new Point(x, y));
			Dock dir = GetDroppedTabDirection(pos.X, pos.Y);

			Invalidate();

			DockedTabControl addTo = m_DockedTabControl;
            if (dir == Dock.Fill && addTo == null)
                return false;

            if (dir != Dock.Fill)
            {
                DockControl dock = GetChildDock(dir);
                addTo = dock.m_DockedTabControl;

                if (!m_DropFar)
                    dock.BringToFront();
                else
                    dock.SendToBack();
            }

            if (p.Name == "TabButtonMove")
            {
                TabButton tabButton = DragAndDrop.SourceControl as TabButton;
                if (null == tabButton)
                    return false;

                addTo.AddPage(tabButton);
            }

            if (p.Name == "TabWindowMove")
            {
                DockedTabControl tabControl = DragAndDrop.SourceControl as DockedTabControl;
                if (null == tabControl)
                    return false;
                if (tabControl == addTo)
                    return false;

                tabControl.MoveTabsTo(addTo);
            }

			return true;
        }

        /// <summary>
        /// Indicates whether the control contains any docked children.
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                if (m_DockedTabControl != null && m_DockedTabControl.TabCount > 0) return false;

                if (m_Left != null && !m_Left.IsEmpty) return false;
                if (m_Right != null && !m_Right.IsEmpty) return false;
                if (m_Top != null && !m_Top.IsEmpty) return false;
                if (m_Bottom != null && !m_Bottom.IsEmpty) return false;

                return true;
            }
        }

		protected virtual void OnTabRemoved(ControlBase control, EventArgs args)
        {
            DoRedundancyCheck();
            DoConsolidateCheck();
        }

        protected virtual void DoRedundancyCheck()
        {
            if (!IsEmpty) return;

            DockControl pDockParent = Parent as DockControl;
            if (null == pDockParent) return;

            pDockParent.OnRedundantChildDock(this);
        }

        protected virtual void DoConsolidateCheck()
        {
            if (IsEmpty) return;
            if (m_DockedTabControl == null) return;
			if (m_DockedTabControl.TabCount > 0) return;

            if (m_Bottom != null && !m_Bottom.IsEmpty)
            {
                m_Bottom.m_DockedTabControl.MoveTabsTo(m_DockedTabControl);
                return;
            }

            if (m_Top != null && !m_Top.IsEmpty)
            {
                m_Top.m_DockedTabControl.MoveTabsTo(m_DockedTabControl);
                return;
            }

            if (m_Left != null && !m_Left.IsEmpty)
            {
                m_Left.m_DockedTabControl.MoveTabsTo(m_DockedTabControl);
                return;
            }

            if (m_Right != null && !m_Right.IsEmpty)
            {
                m_Right.m_DockedTabControl.MoveTabsTo(m_DockedTabControl);
                return;
            }
        }

        protected virtual void OnRedundantChildDock(DockControl dock)
        {
            dock.IsCollapsed = true;
            DoRedundancyCheck();
            DoConsolidateCheck();
        }

        public override void DragAndDrop_HoverEnter(Package p, int x, int y)
        {
            m_DrawHover = true;
        }

        public override void DragAndDrop_HoverLeave(Package p)
        {
            m_DrawHover = false;
        }

        public override void DragAndDrop_Hover(Package p, int x, int y)
        {
            Point pos = CanvasPosToLocal(new Point(x, y));
			Dock dir = GetDroppedTabDirection(pos.X, pos.Y);

            if (dir == Dock.Fill)
            {
                if (null == m_DockedTabControl)
                {
                    m_HoverRect = Rectangle.Empty;
                    return;
                }

                m_HoverRect = InnerBounds;
                return;
            }

            m_HoverRect = RenderBounds;

            int helpBarWidth;

            if (dir == Dock.Left)
            {
                helpBarWidth = (int)(m_HoverRect.Width * 0.25f);
                m_HoverRect.Width = helpBarWidth;
            }

            if (dir == Dock.Right)
            {
                helpBarWidth = (int)(m_HoverRect.Width * 0.25f);
                m_HoverRect.X = m_HoverRect.Width - helpBarWidth;
                m_HoverRect.Width = helpBarWidth;
            }

            if (dir == Dock.Top)
            {
                helpBarWidth = (int)(m_HoverRect.Height * 0.25f);
                m_HoverRect.Height = helpBarWidth;
            }

            if (dir == Dock.Bottom)
            {
                helpBarWidth = (int)(m_HoverRect.Height * 0.25f);
                m_HoverRect.Y = m_HoverRect.Height - helpBarWidth;
                m_HoverRect.Height = helpBarWidth;
            }

            if ((dir == Dock.Top || dir == Dock.Bottom) && !m_DropFar)
            {
                if (m_Left != null && !m_Left.IsCollapsed)
                {
                    m_HoverRect.X += m_Left.ActualWidth;
                    m_HoverRect.Width -= m_Left.ActualWidth;
                }

                if (m_Right != null && !m_Right.IsCollapsed)
                {
                    m_HoverRect.Width -= m_Right.ActualWidth;
                }
            }

            if ((dir == Dock.Left || dir == Dock.Right) && !m_DropFar)
            {
                if (m_Top != null && !m_Top.IsCollapsed)
                {
                    m_HoverRect.Y += m_Top.ActualHeight;
                    m_HoverRect.Height -= m_Top.ActualHeight;
                }

                if (m_Bottom != null && !m_Bottom.IsCollapsed)
                {
                    m_HoverRect.Height -= m_Bottom.ActualHeight;
                }
            }
        }

        /// <summary>
        /// Renders over the actual control (overlays).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderOver(Skin.SkinBase skin)
        {
            if (!m_DrawHover)
                return;

            Renderer.RendererBase render = skin.Renderer;
            render.DrawColor = new Color(20, 255, 200, 255);
            render.DrawFilledRect(RenderBounds);

            if (m_HoverRect.Width == 0)
                return;

            render.DrawColor = new Color(100, 255, 200, 255);
            render.DrawFilledRect(m_HoverRect);

            render.DrawColor = new Color(200, 255, 200, 255);
            render.DrawLinedRect(m_HoverRect);
        }
    }
}
