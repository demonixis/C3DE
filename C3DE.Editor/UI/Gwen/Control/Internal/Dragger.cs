using System;
using Gwen.Control;
using Gwen.Input;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Base for controls that can be dragged by mouse.
    /// </summary>
    public class Dragger : ControlBase
    {
        protected bool m_Held;
        protected Point m_HoldPos;
        protected ControlBase m_Target;

        internal ControlBase Target { get { return m_Target; } set { m_Target = value; } }

        /// <summary>
        /// Indicates if the control is being dragged.
        /// </summary>
        public bool IsHeld { get { return m_Held; } }

        /// <summary>
        /// Event invoked when the control position has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> Dragged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dragger"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Dragger(ControlBase parent) : base(parent)
        {
            MouseInputEnabled = true;
            m_Held = false;
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void OnMouseClickedLeft(int x, int y, bool down)
        {
            if (null == m_Target) return;

            if (down)
            {
                m_Held = true;
                m_HoldPos = m_Target.CanvasPosToLocal(new Point(x, y));
                InputHandler.MouseFocus = this;
            }
            else
            {
                m_Held = false;

                InputHandler.MouseFocus = null;
            }
        }

        /// <summary>
        /// Handler invoked on mouse moved event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X change.</param>
        /// <param name="dy">Y change.</param>
        protected override void OnMouseMoved(int x, int y, int dx, int dy)
        {
            if (null == m_Target) return;
            if (!m_Held) return;

            Point p = new Point(x - m_HoldPos.X, y - m_HoldPos.Y);

            // Translate to parent
            if (m_Target.Parent != null)
                p = m_Target.Parent.CanvasPosToLocal(p);

            //m_Target->SetPosition( p.x, p.y );
            m_Target.MoveTo(p.X, p.Y);
            if (Dragged != null)
				Dragged.Invoke(this, EventArgs.Empty);
        }

		protected override Size OnMeasure(Size availableSize)
		{
			return availableSize;
		}

		protected override Size OnArrange(Size finalSize)
		{
			return finalSize;
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
        {
            
        }
    }
}
