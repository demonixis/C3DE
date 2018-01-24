using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
    /// <summary>
    /// CollapsibleCategory control. Used in CollapsibleList.
    /// </summary>
    public class CollapsibleCategory : ControlBase
    {
        private readonly CategoryHeaderButton m_HeaderButton;
        private readonly CollapsibleList m_List;

        /// <summary>
        /// Header text.
        /// </summary>
        public string Text { get { return m_HeaderButton.Text; } set { m_HeaderButton.Text = value; } }

        /// <summary>
        /// Determines whether the category is collapsed (closed).
        /// </summary>
        public bool IsCategoryCollapsed { get { return m_HeaderButton.ToggleState; } set { m_HeaderButton.ToggleState = value; } }

        /// <summary>
        /// Invoked when an entry has been selected.
        /// </summary>
		public event GwenEventHandler<ItemSelectedEventArgs> Selected;

        /// <summary>
        /// Invoked when the category collapsed state has been changed (header button has been pressed).
        /// </summary>
		public event GwenEventHandler<EventArgs> Collapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollapsibleCategory"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CollapsibleCategory(CollapsibleList parent) : base(parent)
        {
            m_HeaderButton = new CategoryHeaderButton(this);
            m_HeaderButton.Text = "Category Title";
            m_HeaderButton.Toggled += OnHeaderToggle;

            m_List = parent;

            Padding = new Padding(1, 0, 1, 2);
		}

		/// <summary>
		/// Gets the selected entry.
		/// </summary>
		public Button GetSelectedButton()
        {
            foreach (ControlBase child in Children)
            {
                CategoryButton button = child as CategoryButton;
                if (button == null)
                    continue;

                if (button.ToggleState)
                    return button;
            }

            return null;
        }

        /// <summary>
        /// Handler for header button toggle event.
        /// </summary>
        /// <param name="control">Source control.</param>
		protected virtual void OnHeaderToggle(ControlBase control, EventArgs args)
        {
			Invalidate();

            if (Collapsed != null)
				Collapsed.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handler for Selected event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void OnSelected(ControlBase control, EventArgs args)
        {
            CategoryButton child = control as CategoryButton;
            if (child == null) return;

            if (m_List != null)
            {
                m_List.UnselectAll();
            }
            else
            {
                UnselectAll();
            }

            child.ToggleState = true;

            if (Selected != null)
                Selected.Invoke(this, new ItemSelectedEventArgs(control));
        }

        /// <summary>
        /// Adds a new entry.
        /// </summary>
        /// <param name="name">Entry name (displayed).</param>
        /// <returns>Newly created control.</returns>
        public Button Add(string name)
        {
            CategoryButton button = new CategoryButton(this);
            button.Text = name;
            button.Padding = new Padding(5, 2, 2, 2);
            button.Clicked += OnSelected;

			Invalidate();

			return button;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawCategoryInner(this, m_HeaderButton.ActualHeight, m_HeaderButton.ToggleState);
            base.Render(skin);
        }

        /// <summary>
        /// Unselects all entries.
        /// </summary>
        public void UnselectAll()
        {
            foreach (ControlBase child in Children)
            {
                CategoryButton button = child as CategoryButton;
                if (button == null)
                    continue;

                button.ToggleState = false;
            }
        }

		protected override Size OnMeasure(Size availableSize)
		{
			Size headerSize = m_HeaderButton.Measure(availableSize);

			if (IsCategoryCollapsed)
			{
				return headerSize;
			}
			else
			{
				int width = headerSize.Width;
				int height = headerSize.Height + Padding.Top + Padding.Bottom;

				foreach (ControlBase child in Children)
				{
					CategoryButton button = child as CategoryButton;
					if (button == null)
						continue;

					Size size = child.Measure(availableSize);
					if (size.Width > width)
						width = child.Width;
					height += size.Height;
				}

				width += Padding.Left + Padding.Right;

				return new Size(width, height);
			}
		}

		protected override Size OnArrange(Size finalSize)
		{
			m_HeaderButton.Arrange(new Rectangle(0, 0, finalSize.Width, m_HeaderButton.MeasuredSize.Height));

			if (IsCategoryCollapsed)
			{
				return new Size(finalSize.Width, m_HeaderButton.MeasuredSize.Height);
			}
			else
			{
				int y = m_HeaderButton.MeasuredSize.Height + Padding.Top;
				int width = finalSize.Width - Padding.Left - Padding.Right;
				bool b = true;

				foreach (ControlBase child in Children)
				{
					CategoryButton button = child as CategoryButton;
					if (button == null)
						continue;

					button.m_Alt = b;
					button.UpdateColors();
					b = !b;

					child.Arrange(new Rectangle(Padding.Left, y, width, child.MeasuredSize.Height));
					y += child.MeasuredSize.Height;
				}

				y += Padding.Bottom;

				return new Size(finalSize.Width, y);
			}
		}
    }
}
