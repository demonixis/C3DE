using System;
using Gwen.Input;
using Gwen.Control.Internal;

namespace Gwen.Control
{
    /// <summary>
    /// Horizontal scrollbar.
    /// </summary>
    public class HorizontalScrollBar : ScrollBar
    {
        /// <summary>
        /// Bar size (in pixels).
        /// </summary>
        public override int BarSize
        {
            get { return m_Bar.ActualWidth; }
        }

        /// <summary>
        /// Bar position (in pixels).
        /// </summary>
        public override int BarPos
        {
            get { return m_Bar.ActualLeft - ActualHeight; }
        }

        /// <summary>
        /// Indicates whether the bar is horizontal.
        /// </summary>
        public override bool IsHorizontal
        {
            get { return true; }
        }

        /// <summary>
        /// Button size (in pixels).
        /// </summary>
        public override int ButtonSize
        {
            get { return ActualHeight; }
        }

		public override int Height
		{
			get
			{
				return base.Height;
			}

			set
			{
				base.Height = value;

				m_ScrollButton[0].Width = this.Height;
				m_ScrollButton[1].Width = this.Height;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HorizontalScrollBar"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public HorizontalScrollBar(ControlBase parent)
            : base(parent)
        {
			Height = BaseUnit;

			m_Bar.IsHorizontal = true;

			m_ScrollButton[0].Dock = Dock.Left;
			m_ScrollButton[0].SetDirectionLeft();
            m_ScrollButton[0].Clicked += NudgeLeft;

			m_ScrollButton[1].Dock = Dock.Right;
			m_ScrollButton[1].SetDirectionRight();
            m_ScrollButton[1].Clicked += NudgeRight;

			m_Bar.Dock = Dock.Fill;
			m_Bar.Dragged += OnBarMoved;
        }

		protected override Size OnArrange(Size finalSize)
		{
			Size size = base.OnArrange(finalSize);

			SetScrollAmount(ScrollAmount, true);

			return size;
		}

		protected override void UpdateBarSize()
		{
			float barWidth = 0.0f;
			if (m_ContentSize > 0.0f) barWidth = (m_ViewableContentSize / m_ContentSize) * (ActualWidth - (ButtonSize * 2));

			if (barWidth < ButtonSize * 0.5f)
				barWidth = (int)(ButtonSize * 0.5f);

			m_Bar.SetSize((int)barWidth, m_Bar.ActualHeight);
			m_Bar.IsHidden = ActualWidth - (ButtonSize * 2) <= barWidth;

			//Based on our last scroll amount, produce a position for the bar
			if (!m_Bar.IsHeld)
			{
				SetScrollAmount(ScrollAmount, true);
			}
		}

		public void NudgeLeft(ControlBase control, EventArgs args)
        {
            if (!IsDisabled)
                SetScrollAmount(ScrollAmount - NudgeAmount, true);
        }

		public void NudgeRight(ControlBase control, EventArgs args)
        {
            if (!IsDisabled)
                SetScrollAmount(ScrollAmount + NudgeAmount, true);
        }

        public override void ScrollToLeft()
        {
            SetScrollAmount(0, true);
        }

        public override void ScrollToRight()
        {
            SetScrollAmount(1, true);
        }

        public override float NudgeAmount
        {
            get
            {
                if (m_Depressed)
                    return m_ViewableContentSize / m_ContentSize;
                else
                    return base.NudgeAmount;
            }
            set
            {
                base.NudgeAmount = value;
            }
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
            if (down)
            {
                m_Depressed = true;
                InputHandler.MouseFocus = this;
            }
            else
            {
                Point clickPos = CanvasPosToLocal(new Point(x, y));
                if (clickPos.X < m_Bar.ActualLeft)
					NudgeLeft(this, EventArgs.Empty);
                else
                    if (clickPos.X > m_Bar.ActualLeft + m_Bar.ActualWidth)
						NudgeRight(this, EventArgs.Empty);

                m_Depressed = false;
                InputHandler.MouseFocus = null;
            }
        }

        protected override float CalculateScrolledAmount()
        {
			float value = (float)(m_Bar.ActualLeft - ButtonSize) / (ActualWidth - m_Bar.ActualWidth - (ButtonSize * 2));
			if (Single.IsNaN(value))
				value = 0.0f;
			return value;
        }

        /// <summary>
        /// Sets the scroll amount (0-1).
        /// </summary>
        /// <param name="value">Scroll amount.</param>
        /// <param name="forceUpdate">Determines whether the control should be updated.</param>
        /// <returns>
        /// True if control state changed.
        /// </returns>
        public override bool SetScrollAmount(float value, bool forceUpdate = false)
        {
            value = Util.Clamp(value, 0, 1);

            if (!base.SetScrollAmount(value, forceUpdate))
                return false;

            if (forceUpdate)
            {
                int newX = (int)(ButtonSize + (value * ((ActualWidth - m_Bar.ActualWidth) - (ButtonSize * 2))));
                m_Bar.MoveTo(newX, m_Bar.ActualTop);
            }

            return true;
        }

        /// <summary>
        /// Handler for the BarMoved event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected override void OnBarMoved(ControlBase control, EventArgs args)
        {
			if (m_Bar.IsHeld)
			{
				SetScrollAmount(CalculateScrolledAmount(), false);
			}

			base.OnBarMoved(control, EventArgs.Empty);
		}
	}
}
