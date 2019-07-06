using System;
using Gwen.Control.Internal;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Docked tab control.
    /// </summary>
    public class DockedTabControl : TabControl
    {
        private readonly TabTitleBar m_TitleBar;

        /// <summary>
        /// Determines whether the title bar is visible.
        /// </summary>
        public bool TitleBarVisible { get { return !m_TitleBar.IsCollapsed; } set { m_TitleBar.IsCollapsed = !value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockedTabControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public DockedTabControl(ControlBase parent)
            : base(parent)
        {
            Dock = Dock.Fill;

            m_TitleBar = new TabTitleBar(this);
            m_TitleBar.Dock = Dock.Top;
            m_TitleBar.IsCollapsed = true;

			AllowReorder = true;
		}

		internal override void OnTabPressed(ControlBase control, EventArgs args)
		{
			base.OnTabPressed(control, args);

			TabButton button = control as TabButton;
			if (null == button) return;

			m_TitleBar.Text = button.Text;
		}

		protected override Size OnMeasure(Size availableSize)
		{
			TabStrip.Collapse(TabCount <= 1, false);
			UpdateTitleBar();

			return base.OnMeasure(availableSize);
		}

		private void UpdateTitleBar()
        {
            if (CurrentButton == null)
                return;

            m_TitleBar.UpdateFromTab(CurrentButton);
        }

        public override void DragAndDrop_StartDragging(DragDrop.Package package, int x, int y)
        {
            base.DragAndDrop_StartDragging(package, x, y);

			IsCollapsed = true;
            // This hiding our parent thing is kind of lousy.
            Parent.IsCollapsed = true;
        }

        public override void DragAndDrop_EndDragging(bool success, int x, int y)
        {
			Show();
            if (!success)
            {
				Parent.Show();
            }
        }

        internal void MoveTabsTo(DockedTabControl target)
        {
            var children = TabStrip.Children.ToArray(); // copy because collection will be modified
            foreach (ControlBase child in children)
            {
                TabButton button = child as TabButton;
                if (button == null)
                    continue;
                target.AddPage(button);
            }
            Invalidate();
        }
    }
}
