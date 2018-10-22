using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Slider bar.
    /// </summary>
    public class SliderBar : Dragger
    {
        private bool m_bHorizontal;

        /// <summary>
        /// Indicates whether the bar is horizontal.
        /// </summary>
        public bool IsHorizontal { get { return m_bHorizontal; } set { m_bHorizontal = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderBar"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public SliderBar(ControlBase parent)
            : base(parent)
        {
			Size = new Size(BaseUnit);

            Target = this;
            RestrictToParent = true;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawSliderButton(this, IsHeld, IsHorizontal);
        }
    }
}
