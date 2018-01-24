using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
	public enum ScrollBarButtonDirection
	{
		Left, Top, Right, Bottom
	}

	/// <summary>
	/// Scrollbar button.
	/// </summary>
	public class ScrollBarButton : ButtonBase
    {
        private ScrollBarButtonDirection m_Direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrollBarButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ScrollBarButton(ControlBase parent)
            : base(parent)
        {
            SetDirectionUp();
        }

        public virtual void SetDirectionUp()
        {
            m_Direction = ScrollBarButtonDirection.Top;
        }

        public virtual void SetDirectionDown()
        {
            m_Direction = ScrollBarButtonDirection.Bottom;
        }

        public virtual void SetDirectionLeft()
        {
            m_Direction = ScrollBarButtonDirection.Left;
        }

        public virtual void SetDirectionRight()
        {
            m_Direction = ScrollBarButtonDirection.Right;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawScrollButton(this, m_Direction, IsDepressed, IsHovered, IsDisabled);
        }
    }
}
