using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	[Xml.XmlControl]
	public class VerticalSplitter : ControlBase
	{
		private readonly SplitterBar m_HSplitter;
		private readonly ControlBase[] m_Sections;

		private float m_HVal; // 0-1
		private int m_BarSize; // pixels
		private int m_ZoomedSection; // 0-3

		/// <summary>
		/// Splitter position (0 - 1)
		/// </summary>
		[Xml.XmlProperty]
		public float Value { get { return m_HVal; } set { SetHValue(value); } }

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
			get { return m_HSplitter.ShouldDrawBackground; }
			set { m_HSplitter.ShouldDrawBackground = value; }
		}

		/// <summary>
		/// Gets or sets the size of the splitter.
		/// </summary>
		[Xml.XmlProperty]
		public int SplitterSize { get { return m_BarSize; } set { m_BarSize = value; } }

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
		public VerticalSplitter(ControlBase parent)
			: base(parent)
		{
			m_Sections = new ControlBase[2];

			m_HSplitter = new SplitterBar(this);
			m_HSplitter.Dragged += OnHorizontalMoved;
			m_HSplitter.Cursor = Cursor.SizeWE;

			m_HVal = 0.5f;

			SetPanel(0, null);
			SetPanel(1, null);

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
			Invalidate();
		}

		public void SetHValue(float value)
		{
			if (value <= 1f || value >= 0)
				m_HVal = value;

			Invalidate();
		}

		protected void OnHorizontalMoved(ControlBase control, EventArgs args)
		{
			m_HVal = CalculateValueHorizontal();
			Invalidate();
		}

		private float CalculateValueHorizontal()
		{
			return m_HSplitter.ActualLeft / (float)(ActualWidth - m_HSplitter.ActualWidth);
		}

		protected override Size OnMeasure(Size availableSize)
		{
			Size size = Size.Zero;

			m_HSplitter.Measure(new Size(m_BarSize, availableSize.Height));
			size.Width += m_HSplitter.Width;

			int h = (int)((availableSize.Width - m_BarSize) * m_HVal);

			if (m_ZoomedSection == -1)
			{
				if (m_Sections[0] != null)
				{
					m_Sections[0].Measure(new Size(h, availableSize.Height));
					size.Width += m_Sections[0].MeasuredSize.Width;
					size.Height = Math.Max(size.Height, m_Sections[0].MeasuredSize.Height);
				}

				if (m_Sections[1] != null)
				{
					m_Sections[1].Measure(new Size(availableSize.Width - m_BarSize - h, availableSize.Height));
					size.Width += m_Sections[1].MeasuredSize.Width;
					size.Height = Math.Max(size.Height, m_Sections[1].MeasuredSize.Height);
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

			m_HSplitter.Arrange(new Rectangle(h, 0, m_HSplitter.MeasuredSize.Width, finalSize.Height));

			if (m_ZoomedSection == -1)
			{
				if (m_Sections[0] != null)
					m_Sections[0].Arrange(new Rectangle(0, 0, h, finalSize.Height));

				if (m_Sections[1] != null)
					m_Sections[1].Arrange(new Rectangle(h + m_BarSize, 0, finalSize.Width - m_BarSize - h, finalSize.Height));
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
				for (int i = 0; i < 2; i++)
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
			
			for (int i = 0; i < 2; i++)
			{
				if (m_Sections[i] != null)
					m_Sections[i].IsHidden = false;
			}

			Invalidate();
			OnZoomChanged();
		}
	}
}
