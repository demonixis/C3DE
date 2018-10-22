using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Item in CollapsibleCategory.
    /// </summary>
    public class CategoryButton : Button
    {
        internal bool m_Alt; // for alternate coloring

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CategoryButton(ControlBase parent) : base(parent)
        {
            Alignment = Alignment.Left | Alignment.CenterV;
            m_Alt = false;
            IsToggle = true;
            TextPadding = new Padding(3, 0, 3, 0);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            if (m_Alt)
            {
                if (IsDepressed || ToggleState)
                    Skin.Renderer.DrawColor = skin.Colors.Category.LineAlt.Button_Selected;
                else if (IsHovered)
                    Skin.Renderer.DrawColor = skin.Colors.Category.LineAlt.Button_Hover;
                else
                    Skin.Renderer.DrawColor = skin.Colors.Category.LineAlt.Button;
            }
            else
            {
                if (IsDepressed || ToggleState)
                    Skin.Renderer.DrawColor = skin.Colors.Category.Line.Button_Selected;
                else if (IsHovered)
                    Skin.Renderer.DrawColor = skin.Colors.Category.Line.Button_Hover;
                else
                    Skin.Renderer.DrawColor = skin.Colors.Category.Line.Button;
            }

            skin.Renderer.DrawFilledRect(RenderBounds);
        }

        /// <summary>
        /// Updates control colors.
        /// </summary>
        public override void UpdateColors()
        {
            if (m_Alt)
            {
                if (IsDepressed || ToggleState)
                {
                    TextColor = Skin.Colors.Category.LineAlt.Text_Selected;
                    return;
                }
                if (IsHovered)
                {
                    TextColor = Skin.Colors.Category.LineAlt.Text_Hover;
                    return;
                }
                TextColor = Skin.Colors.Category.LineAlt.Text;
                return;
            }

            if (IsDepressed || ToggleState)
            {
                TextColor = Skin.Colors.Category.Line.Text_Selected;
                return;
            }
            if (IsHovered)
            {
                TextColor = Skin.Colors.Category.Line.Text_Hover;
                return;
            }
            TextColor = Skin.Colors.Category.Line.Text;
        }
    }
}
