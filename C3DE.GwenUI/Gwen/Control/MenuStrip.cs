using System;
using Gwen.Control.Layout;

namespace Gwen.Control
{
    /// <summary>
    /// Menu strip.
    /// </summary>
	[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
    public class MenuStrip : Menu
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuStrip"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public MenuStrip(ControlBase parent)
            : base(parent)
        {
			Collapse(false, false);

            Padding = new Padding(5, 0, 0, 0);
			IconMarginDisabled = true;
			EnableScroll(true, false);

			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Top;

			m_Layout.Horizontal = true;
			m_Layout.HorizontalAlignment = HorizontalAlignment.Left;
			m_Layout.VerticalAlignment = VerticalAlignment.Stretch;
		}

		/// <summary>
		/// Closes the current menu.
		/// </summary>
		public override void Close()
        {
            
        }

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void RenderUnder(Skin.SkinBase skin)
        {
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawMenuStrip(this);
        }

		/// <summary>
		/// Determines whether the menu should open on mouse hover.
		/// </summary>
		protected override bool ShouldHoverOpenMenu
        {
            get { return IsMenuOpen(); }
        }

        /// <summary>
        /// Add item handler.
        /// </summary>
        /// <param name="item">Item added.</param>
        protected override void OnAddItem(MenuItem item)
        {
            item.TextPadding = new Padding(5, 0, 5, 0);
            item.Padding = new Padding(4, 4, 4, 4);
            item.HoverEnter += OnHoverItem;
        }

		internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent)
		{
			MenuStrip element = new MenuStrip(parent);
			parser.ParseAttributes(element);
			if (parser.MoveToContent())
			{
				foreach (string elementName in parser.NextElement())
				{
					if (elementName == "MenuItem")
					{
						element.AddItem(parser.ParseElement<MenuItem>(element));
					}
				}
			}
			return element;
		}
	}
}
