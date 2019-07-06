using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Label for PropertyRow.
    /// </summary>
    public class PropertyRowLabel : Label
    {
        private readonly PropertyRow m_PropertyRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRowLabel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyRowLabel(PropertyRow parent) : base(parent)
        {
			//AutoSizeToContents = false;
            Alignment = Alignment.Left | Alignment.CenterV;
            m_PropertyRow = parent;
        }

        /// <summary>
        /// Updates control colors.
        /// </summary>
        public override void UpdateColors()
        {
            if (IsDisabled)
            {
                TextColor = Skin.Colors.Button.Disabled;
                return;
            }

            if (m_PropertyRow != null && m_PropertyRow.IsEditing)
            {
                TextColor = Skin.Colors.Properties.Label_Selected;
                return;
            }

            if (m_PropertyRow != null && m_PropertyRow.IsHovered)
            {
                TextColor = Skin.Colors.Properties.Label_Hover;
                return;
            }

            TextColor = Skin.Colors.Properties.Label_Normal;
        }
    }
}
