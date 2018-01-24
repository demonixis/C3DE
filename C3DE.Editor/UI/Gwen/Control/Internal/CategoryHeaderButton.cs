using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Header of CollapsibleCategory.
    /// </summary>
    public class CategoryHeaderButton : Button
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryHeaderButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CategoryHeaderButton(ControlBase parent)
            : base(parent)
        {
            ShouldDrawBackground = false;
            IsToggle = true;
            Alignment = Alignment.Center;
            TextPadding = new Padding(3, 0, 3, 0);
        }

        /// <summary>
        /// Updates control colors.
        /// </summary>
        public override void UpdateColors()
        {
            if (IsDepressed || ToggleState)
                TextColor = Skin.Colors.Category.Header_Closed;
            else
                TextColor = Skin.Colors.Category.Header;
        }
    }
}
