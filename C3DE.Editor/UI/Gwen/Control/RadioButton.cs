using System;

namespace Gwen.Control
{
    /// <summary>
    /// Radio button.
    /// </summary>
    public class RadioButton : CheckBox
    {
        /// <summary>
        /// Determines whether unchecking is allowed.
        /// </summary>
        protected override bool AllowUncheck
        {
            get { return false; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public RadioButton(ControlBase parent)
            : base(parent)
        {
            MouseInputEnabled = true;
            IsTabable = false;
			IsToggle = true; //[halfofastaple] technically true. "Toggle" isn't the best word, "Sticky" is a better one.
        }

		protected override Size OnMeasure(Size availableSize)
		{
			return new Size(15, 15);
		}

		protected override Size OnArrange(Size finalSize)
		{
			return MeasuredSize;
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawRadioButton(this, IsChecked, IsDepressed);
        }
    }
}
