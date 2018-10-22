using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Horizontal slider.
	/// </summary>
	[Xml.XmlControl]
	public class HorizontalSlider : Slider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HorizontalSlider"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public HorizontalSlider(ControlBase parent)
            : base(parent)
        {
			Height = BaseUnit;

            m_SliderBar.IsHorizontal = true;
        }

        protected override float CalculateValue()
        {
            return (float)m_SliderBar.ActualLeft / (ActualWidth - m_SliderBar.ActualWidth);
        }

        protected override void UpdateBarFromValue()
        {
            m_SliderBar.MoveTo((int)((ActualWidth - m_SliderBar.ActualWidth) * (m_Value)), (this.ActualHeight - m_SliderBar.ActualHeight) / 2);
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
            m_SliderBar.MoveTo((int)(CanvasPosToLocal(new Point(x, y)).X - m_SliderBar.ActualWidth / 2), (this.ActualHeight - m_SliderBar.ActualHeight) / 2);
            m_SliderBar.InputMouseClickedLeft(x, y, down);
            OnMoved(m_SliderBar, EventArgs.Empty);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawSlider(this, true, m_SnapToNotches ? m_NotchCount : 0, m_SliderBar.ActualWidth);
        }
    }
}
