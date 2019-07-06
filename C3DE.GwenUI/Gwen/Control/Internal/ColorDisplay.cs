using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
    /// <summary>
    /// Color square.
    /// </summary>
    public class ColorDisplay : ControlBase
    {
        private Color m_Color;
        //private bool m_DrawCheckers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDisplay"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ColorDisplay(ControlBase parent) : base(parent)
        {
			Size = new Size(BaseUnit * 2);
			m_Color = new Color(255, 255, 0, 0);
            //m_DrawCheckers = true;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawColorDisplay(this, m_Color);
        }

        /// <summary>
        /// Current color.
        /// </summary>
        public Color Color { get { return m_Color; } set { m_Color = value; } }
        //public bool DrawCheckers { get { return m_DrawCheckers; } set { m_DrawCheckers = value; } }
        public int R { get { return m_Color.R; } set { m_Color = new Color(m_Color.A, value, m_Color.G, m_Color.B); } }
        public int G { get { return m_Color.G; } set { m_Color = new Color(m_Color.A, m_Color.R, value, m_Color.B); } }
        public int B { get { return m_Color.B; } set { m_Color = new Color(m_Color.A, m_Color.R, m_Color.G, value); } }
        public int A { get { return m_Color.A; } set { m_Color = new Color(value, m_Color.R, m_Color.G, m_Color.B); } }
    }
}
