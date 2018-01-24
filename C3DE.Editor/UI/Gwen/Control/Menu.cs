using System;
using System.Linq;
using Gwen.Control.Internal;
using Gwen.Control.Layout;

namespace Gwen.Control
{
	/// <summary>
	/// Popup menu.
	/// </summary>
	[Xml.XmlControl]
	public class Menu : ScrollControl
    {
		protected StackLayout m_Layout;

		private bool m_DisableIconMargin;
        private bool m_DeleteOnClose;

		private MenuItem m_ParentMenuItem;

		internal override bool IsMenuComponent { get { return true; } }

		/// <summary>
		/// Parent menu item that owns the menu if this is a child of the menu item.
		/// Real parent of the menu is the canvas.
		/// </summary>
		public MenuItem ParentMenuItem { get { return m_ParentMenuItem; } internal set { m_ParentMenuItem = value; } }

		[Xml.XmlProperty]
		public bool IconMarginDisabled { get { return m_DisableIconMargin; } set { m_DisableIconMargin = value; } }

		/// <summary>
		/// Determines whether the menu should be disposed on close.
		/// </summary>
		[Xml.XmlProperty]
		public bool DeleteOnClose { get { return m_DeleteOnClose; } set { m_DeleteOnClose = value; } }

        /// <summary>
        /// Determines whether the menu should open on mouse hover.
        /// </summary>
        protected virtual bool ShouldHoverOpenMenu { get { return true; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Menu"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Menu(ControlBase parent)
            : base(parent)
        {
            Padding = Padding.Two;

			Collapse(true, false);

			IconMarginDisabled = false;

            AutoHideBars = true;
            EnableScroll(false, true);
            DeleteOnClose = false;

			this.HorizontalAlignment = HorizontalAlignment.Left;
			this.VerticalAlignment = VerticalAlignment.Top;

			m_Layout = new StackLayout(this);
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawMenu(this, IconMarginDisabled);
        }

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderUnder(Skin.SkinBase skin)
        {
            base.RenderUnder(skin);
            skin.DrawShadow(this);
        }

        /// <summary>
        ///  Opens the menu.
        /// </summary>
        public void Open()
        {
			Show();
            BringToFront();
            Point mouse = Input.InputHandler.MousePosition;
            SetPosition(mouse.X, mouse.Y);
        }

		protected override Size OnMeasure(Size availableSize)
		{
			availableSize.Height = Math.Min(availableSize.Height, GetCanvas().ActualHeight - this.Top);

			Size size = base.OnMeasure(availableSize);

			size.Width = Math.Min(this.Content.MeasuredSize.Width + Padding.Left + Padding.Right, availableSize.Width);
			size.Height = Math.Min(this.Content.MeasuredSize.Height + Padding.Top + Padding.Bottom, availableSize.Height);

			return size;
		}

        /// <summary>
        /// Adds a new menu item.
        /// </summary>
        /// <param name="text">Item text.</param>
        /// <param name="iconName">Icon texture name.</param>
        /// <param name="accelerator">Accelerator for this item.</param>
        /// <returns>Newly created control.</returns>
        public virtual MenuItem AddItem(string text, string iconName = null, string accelerator = null)
        {
            MenuItem item = new MenuItem(this);
            item.Padding = Padding.Three;
            item.Text = text;
			if (!String.IsNullOrWhiteSpace(iconName))
	            item.SetImage(iconName, ImageAlign.Left | ImageAlign.CenterV);
			if (!String.IsNullOrWhiteSpace(accelerator))
	            item.SetAccelerator(accelerator);

            OnAddItem(item);

            return item;
        }

		/// <summary>
		/// Adds a menu item.
		/// </summary>
		/// <param name="item">Item.</param>
		public virtual void AddItem(MenuItem item)
		{
			item.Parent = this;

			item.Padding = Padding.Three;

			OnAddItem(item);
		}

		/// <summary>
		/// Add item handler.
		/// </summary>
		/// <param name="item">Item added.</param>
		protected virtual void OnAddItem(MenuItem item)
        {
            item.TextPadding = new Padding(IconMarginDisabled ? 0 : 24, 0, 16, 0);
            item.Alignment = Alignment.CenterV | Alignment.Left;
            item.HoverEnter += OnHoverItem;
        }

        /// <summary>
        /// Closes all submenus.
        /// </summary>
        public virtual void CloseAll()
        {
			foreach (var child in Children)
			{
				if (child is MenuItem)
					(child as MenuItem).CloseMenu();
			}
        }

        /// <summary>
        /// Indicates whether any (sub)menu is open.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMenuOpen()
        {
            return Children.Any(child => { if (child is MenuItem) return (child as MenuItem).IsMenuOpen; return false; });
        }

        /// <summary>
        /// Mouse hover handler.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void OnHoverItem(ControlBase control, EventArgs args)
        {
            if (!ShouldHoverOpenMenu) return;

            MenuItem item = control as MenuItem;
            if (null == item) return;
            if (item.IsMenuOpen) return;

            CloseAll();
            item.OpenMenu();
        }

        /// <summary>
        /// Closes the current menu.
        /// </summary>
        public virtual void Close()
        {
            IsCollapsed = true;
            if (DeleteOnClose)
            {
                DelayedDelete();
            }
        }

        /// <summary>
        /// Closes all submenus and the current menu.
        /// </summary>
        public override void CloseMenus()
        {
            base.CloseMenus();
            CloseAll();
            Close();
        }

        /// <summary>
        /// Adds a divider menu item.
        /// </summary>
        public virtual void AddDivider()
        {
            MenuDivider divider = new MenuDivider(this);
            divider.Margin = new Margin(IconMarginDisabled ? 0 : 24, 0, 4, 0);
        }

		/// <summary>
		/// Removes all items.
		/// </summary>
		public void RemoveAll()
		{
			m_Layout.DeleteAllChildren();
		}
    }
}
