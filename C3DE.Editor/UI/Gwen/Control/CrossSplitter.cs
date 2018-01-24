using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
    /// <summary>
    /// Splitter control.
    /// </summary>
	[Xml.XmlControl]
    public class CrossSplitter : ControlBase
    {
        private readonly SplitterBar m_VSplitter;
        private readonly SplitterBar m_HSplitter;
        private readonly SplitterBar m_CSplitter;

        private readonly ControlBase[] m_Sections;

        private float m_HVal; // 0-1
        private float m_VVal; // 0-1
        private int m_BarSize; // pixels

        private int m_ZoomedSection; // 0-3

        /// <summary>
        /// Invoked when one of the panels has been zoomed (maximized).
        /// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> PanelZoomed;

		/// <summary>
		/// Invoked when one of the panels has been unzoomed (restored).
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> PanelUnZoomed;

		/// <summary>
		/// Invoked when the zoomed panel has been changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> ZoomChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossSplitter"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CrossSplitter(ControlBase parent)
            : base(parent)
        {
            m_Sections = new ControlBase[4];

            m_VSplitter = new SplitterBar(this);
            m_VSplitter.Dragged += OnVerticalMoved;
            m_VSplitter.Cursor = Cursor.SizeNS;

            m_HSplitter = new SplitterBar(this);
            m_HSplitter.Dragged += OnHorizontalMoved;
            m_HSplitter.Cursor = Cursor.SizeWE;

            m_CSplitter = new SplitterBar(this);
            m_CSplitter.Dragged += OnCenterMoved;
            m_CSplitter.Cursor = Cursor.SizeAll;

            m_HVal = 0.5f;
            m_VVal = 0.5f;

            SetPanel(0, null);
            SetPanel(1, null);
            SetPanel(2, null);
            SetPanel(3, null);

            SplitterSize = 5;
            SplittersVisible = false;

            m_ZoomedSection = -1;
        }

        /// <summary>
        /// Centers the panels so that they take even amount of space.
        /// </summary>
        public void CenterPanels()
        {
            m_HVal = 0.5f;
            m_VVal = 0.5f;
            Invalidate();
        }

        /// <summary>
        /// Indicates whether any of the panels is zoomed.
        /// </summary>
        public bool IsZoomed { get { return m_ZoomedSection != -1; } }

		/// <summary>
		/// Gets or sets a value indicating whether splitters should be visible.
		/// </summary>
		[Xml.XmlProperty]
		public bool SplittersVisible
        {
            get { return m_CSplitter.ShouldDrawBackground; }
            set 
            {
                m_CSplitter.ShouldDrawBackground = value;
                m_VSplitter.ShouldDrawBackground = value;
                m_HSplitter.ShouldDrawBackground = value;
            }
        }

		/// <summary>
		/// Gets or sets the size of the splitter.
		/// </summary>
		[Xml.XmlProperty]
		public int SplitterSize { get { return m_BarSize; } set { m_BarSize = value; } }

		protected void OnCenterMoved(ControlBase control, EventArgs args)
        {
            CalculateValueCenter();
			Invalidate();
        }

        protected void OnVerticalMoved(ControlBase control, EventArgs args)
        {
            m_VVal = CalculateValueVertical();
			Invalidate();
		}

		protected void OnHorizontalMoved(ControlBase control, EventArgs args)
        {
            m_HVal = CalculateValueHorizontal();
			Invalidate();
		}

		private void CalculateValueCenter()
        {
            m_HVal = m_CSplitter.ActualLeft / (float)(ActualWidth - m_CSplitter.ActualWidth);
            m_VVal = m_CSplitter.ActualTop / (float)(ActualHeight - m_CSplitter.ActualHeight);
        }

        private float CalculateValueVertical()
        {
            return m_VSplitter.ActualTop / (float)(ActualHeight - m_VSplitter.ActualHeight);
        }

        private float CalculateValueHorizontal()
        {
            return m_HSplitter.ActualLeft / (float)(ActualWidth - m_HSplitter.ActualWidth);
        }

		protected override Size OnMeasure(Size availableSize)
		{
			Size size = Size.Zero;

			m_VSplitter.Measure(new Size(availableSize.Width, m_BarSize));
			m_HSplitter.Measure(new Size(m_BarSize, availableSize.Height));
			m_CSplitter.Measure(new Size(m_BarSize, m_BarSize));
			size = new Size(m_HSplitter.Width, m_VSplitter.Height);

			int h = (int)((availableSize.Width - m_BarSize) * m_HVal);
			int v = (int)((availableSize.Height - m_BarSize) * m_VVal);

			if (m_ZoomedSection == -1)
			{
				if (m_Sections[0] != null)
				{
					m_Sections[0].Measure(new Size(h, v));
					size += m_Sections[0].MeasuredSize;
				}
				if (m_Sections[1] != null)
				{
					m_Sections[1].Measure(new Size(availableSize.Width - m_BarSize - h, v));
					size += m_Sections[1].MeasuredSize;
				}
				if (m_Sections[2] != null)
				{
					m_Sections[2].Measure(new Size(h, availableSize.Height - m_BarSize - v));
					size += m_Sections[2].MeasuredSize;
				}
				if (m_Sections[3] != null)
				{
					m_Sections[3].Measure(new Size(availableSize.Width - m_BarSize - h, availableSize.Height - m_BarSize - v));
					size += m_Sections[3].MeasuredSize;
				}
			}
			else
			{
				m_Sections[m_ZoomedSection].Measure(availableSize);
				size = m_Sections[m_ZoomedSection].MeasuredSize;
			}

			return size;
		}

		protected override Size OnArrange(Size finalSize)
		{
			int h = (int)((finalSize.Width - m_BarSize) * m_HVal);
			int v = (int)((finalSize.Height - m_BarSize) * m_VVal);

			m_VSplitter.Arrange(new Rectangle(0, v, m_VSplitter.MeasuredSize.Width, m_VSplitter.MeasuredSize.Height));
			m_HSplitter.Arrange(new Rectangle(h, 0, m_HSplitter.MeasuredSize.Width, m_HSplitter.MeasuredSize.Height));
			m_CSplitter.Arrange(new Rectangle(h, v, m_CSplitter.MeasuredSize.Width, m_CSplitter.MeasuredSize.Height));

			if (m_ZoomedSection == -1)
			{
				if (m_Sections[0] != null)
					m_Sections[0].Arrange(new Rectangle(0, 0, h, v));

				if (m_Sections[1] != null)
					m_Sections[1].Arrange(new Rectangle(h + m_BarSize, 0, finalSize.Width - m_BarSize - h, v));

				if (m_Sections[2] != null)
					m_Sections[2].Arrange(new Rectangle(0, v + m_BarSize, h, finalSize.Height - m_BarSize - v));

				if (m_Sections[3] != null)
					m_Sections[3].Arrange(new Rectangle(h + m_BarSize, v + m_BarSize, finalSize.Width - m_BarSize - h, finalSize.Height - m_BarSize - v));
			}
			else
			{
				m_Sections[m_ZoomedSection].Arrange(new Rectangle(0, 0, finalSize.Width, finalSize.Height));
			}

			return finalSize;
		}

        /// <summary>
        /// Assigns a control to the specific inner section.
        /// </summary>
        /// <param name="index">Section index (0-3).</param>
        /// <param name="panel">Control to assign.</param>
        public void SetPanel(int index, ControlBase panel)
        {
            m_Sections[index] = panel;

            if (panel != null)
            {
                panel.Parent = this;
            }

			Invalidate();
		}

		/// <summary>
		/// Gets the specific inner section.
		/// </summary>
		/// <param name="index">Section index (0-3).</param>
		/// <returns>Specified section.</returns>
		public ControlBase GetPanel(int index)
        {
            return m_Sections[index];
        }

		protected override void OnChildAdded(ControlBase child)
		{
			if (!(child is SplitterBar))
			{
				if (m_Sections[0] == null)
					SetPanel(0, child);
				else if (m_Sections[1] == null)
					SetPanel(1, child);
				else if (m_Sections[2] == null)
					SetPanel(2, child);
				else if (m_Sections[3] == null)
					SetPanel(3, child);
				else
					throw new Exception("Too many panels added.");
			}

			base.OnChildAdded(child);
		}

		/// <summary>
		/// Internal handler for the zoom changed event.
		/// </summary>
		protected void OnZoomChanged()
        {
            if (ZoomChanged != null)
				ZoomChanged.Invoke(this, EventArgs.Empty);
         
            if (m_ZoomedSection == -1)
            {
                if (PanelUnZoomed != null)
					PanelUnZoomed.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (PanelZoomed != null)
					PanelZoomed.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Maximizes the specified panel so it fills the entire control.
        /// </summary>
        /// <param name="section">Panel index (0-3).</param>
        public void Zoom(int section)
        {
            UnZoom();

            if (m_Sections[section] != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i != section && m_Sections[i] != null)
                        m_Sections[i].IsHidden = true;
                }
                m_ZoomedSection = section;

				Invalidate();
			}
			OnZoomChanged();
        }

        /// <summary>
        /// Restores the control so all panels are visible.
        /// </summary>
        public void UnZoom()
        {
            m_ZoomedSection = -1;

            for (int i = 0; i < 4; i++)
            {
                if (m_Sections[i] != null)
                    m_Sections[i].IsHidden = false;
            }

			Invalidate();
			OnZoomChanged();
        }
    }
}
