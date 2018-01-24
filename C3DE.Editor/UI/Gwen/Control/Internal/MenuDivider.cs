using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Divider menu item.
    /// </summary>
    public class MenuDivider : ControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDivider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public MenuDivider(ControlBase parent)
            : base(parent)
        {
        }

		protected override Size OnMeasure(Size availableSize)
		{
			return new Size(10, 1);
		}

		protected override Size OnArrange(Size finalSize)
		{
			return new Size(finalSize.Width, 1);
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawMenuDivider(this);
        }
    }
}
