using System;

namespace Gwen.Control.Layout
{
    /// <summary>
    /// Base splitter class.
    /// </summary>
    public class Splitter : ControlBase
    {
        private readonly ControlBase[] m_Panel;
        private readonly bool[] m_Scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="Splitter"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Splitter(ControlBase parent) : base(parent)
        {
            m_Panel = new ControlBase[2];
            m_Scale = new bool[2];
            m_Scale[0] = true;
            m_Scale[1] = true;
        }

        /// <summary>
        /// Sets the contents of a splitter panel.
        /// </summary>
        /// <param name="panelIndex">Panel index (0-1).</param>
        /// <param name="panel">Panel contents.</param>
        /// <param name="noScale">Determines whether the content is to be scaled.</param>
        public void SetPanel(int panelIndex, ControlBase panel, bool noScale = false)
        {
            if (panelIndex < 0 || panelIndex > 1)
                throw new ArgumentException("Invalid panel index", "panelIndex");

            m_Panel[panelIndex] = panel;
            m_Scale[panelIndex] = !noScale;

            if (null != m_Panel[panelIndex])
            {
                m_Panel[panelIndex].Parent = this;
            }
        }

        /// <summary>
        /// Gets the contents of a secific panel.
        /// </summary>
        /// <param name="panelIndex">Panel index (0-1).</param>
        /// <returns></returns>
        ControlBase GetPanel(int panelIndex)
        {
            if (panelIndex < 0 || panelIndex > 1)
                throw new ArgumentException("Invalid panel index", "panelIndex");
            return m_Panel[panelIndex];
        }

		protected override Size OnMeasure(Size availableSize)
		{
			Size size = Size.Zero;

			if (m_Panel[0] != null)
			{
				m_Panel[0].Measure(new Size(availableSize.Width, availableSize.Height / 2));
				size = m_Panel[0].MeasuredSize;
			}

			if (m_Panel[1] != null)
			{
				m_Panel[1].Measure(new Size(availableSize.Width, availableSize.Height / 2));
				size.Width = Math.Max(size.Width, m_Panel[1].MeasuredSize.Width);
				size.Height += m_Panel[1].MeasuredSize.Height;
			}

			return size;
		}

		protected override Size OnArrange(Size finalSize)
		{
			int y = 0;

			if (m_Panel[0] != null)
			{
				m_Panel[0].Arrange(new Rectangle(0, 0, finalSize.Width, finalSize.Height / 2));
				y = m_Panel[0].ActualHeight;
			}

			if (m_Panel[1] != null)
			{
				m_Panel[1].Arrange(new Rectangle(0, y, finalSize.Width, finalSize.Height / 2));
				y += m_Panel[1].ActualHeight;
			}

			return new Size(finalSize.Width, y);
		}
    }
}
