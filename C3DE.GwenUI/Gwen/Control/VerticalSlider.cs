using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Vertical slider.
	/// </summary>
	[Xml.XmlControl]
	public class VerticalSlider : Slider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VerticalSlider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public VerticalSlider(ControlBase parent)
            : base(parent)
        {
			Width = BaseUnit;

            m_SliderBar.IsHorizontal = false;
        }

        protected override float CalculateValue()
        {
            return 1 - m_SliderBar.ActualTop / (float)(ActualHeight - m_SliderBar.ActualHeight);
        }

        protected override void UpdateBarFromValue()
        {
            m_SliderBar.MoveTo((this.ActualWidth - m_SliderBar.ActualWidth) / 2, (int)((ActualHeight - m_SliderBar.ActualHeight) * (1 - m_Value)));
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
			base.OnMouseClickedLeft(x, y, down);
            m_SliderBar.MoveTo((this.ActualWidth - m_SliderBar.ActualWidth) / 2, (int) (CanvasPosToLocal(new Point(x, y)).Y - m_SliderBar.ActualHeight*0.5));
            m_SliderBar.InputMouseClickedLeft(x, y, down);
            OnMoved(m_SliderBar, EventArgs.Empty);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawSlider(this, false, m_SnapToNotches ? m_NotchCount : 0, m_SliderBar.ActualHeight);
        }
    }
}
