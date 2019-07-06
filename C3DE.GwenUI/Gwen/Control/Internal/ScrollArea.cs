using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
	public class ScrollArea : InnerContentControl
	{
		private bool m_CanScrollH;
		private bool m_CanScrollV;

		public ScrollArea(ControlBase parent)
			: base(parent)
		{
			m_CanScrollV = true;
			m_CanScrollH = true;
		}

		public Size ViewableContentSize { get; private set; }

		public Size ContentSize { get { return new Size(m_InnerPanel.ActualWidth, m_InnerPanel.ActualHeight); } }

		public Point ScrollPosition
		{
			get { return m_InnerPanel.ActualPosition; }
			set
			{
				SetScrollPosition(value.X, value.Y);
			}
		}

		public int VerticalScroll
		{
			get
			{
				return m_InnerPanel.ActualTop;
			}
			set
			{

				m_InnerPanel.SetPosition(Content.ActualLeft, value);
			}
		}

		public int HorizontalScroll
		{
			get
			{
				return m_InnerPanel.ActualLeft;
			}
			set
			{
				m_InnerPanel.SetPosition(value, m_InnerPanel.ActualTop);
			}
		}

		public virtual void EnableScroll(bool horizontal, bool vertical)
		{
			m_CanScrollV = vertical;
			m_CanScrollH = horizontal;
		}

		public void SetScrollPosition(int horizontal, int vertical)
		{
			m_InnerPanel.SetPosition(horizontal, vertical);
		}

		protected override Size OnMeasure(Size availableSize)
		{
			if (m_InnerPanel == null)
				return Size.Zero;

			Size size = m_InnerPanel.Measure(new Size(m_CanScrollH ? Util.Infinity : availableSize.Width, m_CanScrollV ? Util.Infinity : availableSize.Height));

			// Let the parent determine the size if scrolling is enabled
			size.Width = m_CanScrollH ? 0 : Math.Min(size.Width, availableSize.Width);
			size.Height = m_CanScrollV ? 0 : Math.Min(size.Height, availableSize.Height);

			return size;
		}

		protected override Size OnArrange(Size finalSize)
		{
			if (m_InnerPanel == null)
				return finalSize;

			int scrollAreaWidth = Math.Max(finalSize.Width, m_InnerPanel.MeasuredSize.Width);
			int scrollAreaHeight = Math.Max(finalSize.Height, m_InnerPanel.MeasuredSize.Height);

			m_InnerPanel.Arrange(new Rectangle(0, 0, scrollAreaWidth, scrollAreaHeight));

			this.ViewableContentSize = new Size(finalSize.Width, finalSize.Height);

			return finalSize;
		}
	}
}
