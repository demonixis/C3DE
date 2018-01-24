using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Base for controls whose interior can be scrolled.
	/// </summary>
	[Xml.XmlControl]
	public class ScrollControl : ContentControl
	{
		private readonly ScrollBar m_VerticalScrollBar;
		private readonly ScrollBar m_HorizontalScrollBar;
		private readonly ScrollArea m_ScrollArea;

		private bool m_CanScrollH;
		private bool m_CanScrollV;
		private bool m_AutoHideBars;

		private bool m_AutoSizeToContent;

		public int VerticalScroll
		{
			get
			{
				return -m_ScrollArea.VerticalScroll;
			}
			set
			{
				m_VerticalScrollBar.SetScrollAmount(value / (float)(ContentSize.Height - ViewableContentSize.Height), true);
			}
		}

		public int HorizontalScroll
		{
			get
			{
				return -m_ScrollArea.HorizontalScroll;
			}
			set
			{
				m_HorizontalScrollBar.SetScrollAmount(value / (float)(ContentSize.Width - ViewableContentSize.Width), true);
			}
		}

		public override ControlBase Content
		{
			get { return m_ScrollArea.Content; }
		}

		protected ControlBase Container { get { return m_ScrollArea; } }

		/// <summary>
		/// Indicates whether the control can be scrolled horizontally.
		/// </summary>
		[Xml.XmlProperty]
		public bool CanScrollH { get { return m_CanScrollH; } set { EnableScroll(value, m_CanScrollV); } }

		/// <summary>
		/// Indicates whether the control can be scrolled vertically.
		/// </summary>
		[Xml.XmlProperty]
		public bool CanScrollV { get { return m_CanScrollV; } set { EnableScroll(m_CanScrollH, value); } }

		/// <summary>
		/// If set, try to set the control size the same as the content size. If it doesn't fit, enable scrolling.
		/// </summary>
		/// <remarks>Try to avoid setting this. This can cause a total invalidation of all controls.</remarks>
		[Xml.XmlProperty]
		public bool AutoSizeToContent { get { return m_AutoSizeToContent; } set { if (value == m_AutoSizeToContent) return; m_AutoSizeToContent = value; IsVirtualControl = !value; Invalidate(); } }

		/// <summary>
		/// Determines whether the scroll bars should be hidden if not needed.
		/// </summary>
		[Xml.XmlProperty]
		public bool AutoHideBars { get { return m_AutoHideBars; } set { m_AutoHideBars = value; } }

		public Size ViewableContentSize { get { return m_ScrollArea.ViewableContentSize; } }

		public Size ContentSize { get { return m_ScrollArea.ContentSize; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="ScrollControl"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ScrollControl(ControlBase parent)
			: base(parent)
		{
			MouseInputEnabled = true;

			m_CanScrollV = true;
			m_CanScrollH = true;

			m_AutoHideBars = false;
			m_AutoSizeToContent = false;

			m_VerticalScrollBar = new VerticalScrollBar(this);
			m_VerticalScrollBar.BarMoved += VBarMoved;
			m_VerticalScrollBar.NudgeAmount = 30;

			m_HorizontalScrollBar = new HorizontalScrollBar(this);
			m_HorizontalScrollBar.BarMoved += HBarMoved;
			m_HorizontalScrollBar.NudgeAmount = 30;

			m_ScrollArea = new ScrollArea(this);
			m_ScrollArea.SendToBack();

			m_InnerPanel = m_ScrollArea;

			IsVirtualControl = true;
		}

		/// <summary>
		/// Enables or disables inner scrollbars.
		/// </summary>
		/// <param name="horizontal">Determines whether the horizontal scrollbar should be enabled.</param>
		/// <param name="vertical">Determines whether the vertical scrollbar should be enabled.</param>
		public virtual void EnableScroll(bool horizontal, bool vertical)
		{
			m_CanScrollV = vertical;
			m_CanScrollH = horizontal;
			//m_VerticalScrollBar.IsHidden = !m_CanScrollV;
			//m_HorizontalScrollBar.IsHidden = !m_CanScrollH;

			m_ScrollArea.EnableScroll(horizontal, vertical);

			Invalidate();
		}

		protected virtual void VBarMoved(ControlBase control, EventArgs args)
		{
			UpdateScrollArea();
		}

		protected virtual void HBarMoved(ControlBase control, EventArgs args)
		{
			UpdateScrollArea();
		}

		protected override Size OnMeasure(Size availableSize)
		{
			Size innerSize = availableSize - Padding;

			// Check if scroll bars visible because of auto hide flag not set
			bool needScrollH = m_CanScrollH && !m_AutoHideBars;
			bool needScrollV = m_CanScrollV && !m_AutoHideBars;

			Size scrollAreaSize = innerSize;

			if (needScrollH)
			{
				m_HorizontalScrollBar.Measure(scrollAreaSize);
				scrollAreaSize.Height = innerSize.Height - m_HorizontalScrollBar.MeasuredSize.Height;
			}
			if (needScrollV)
			{
				m_VerticalScrollBar.Measure(scrollAreaSize);
				scrollAreaSize.Width = innerSize.Width - m_VerticalScrollBar.MeasuredSize.Width;
				// Re-measure horizontal scroll bar to take into account the width of the vertical scroll bar
				if (needScrollH)
				{
					m_HorizontalScrollBar.Measure(scrollAreaSize);
					scrollAreaSize.Height = innerSize.Height - m_HorizontalScrollBar.MeasuredSize.Height;
				}
			}

			m_ScrollArea.Measure(scrollAreaSize);
			Size contentSize = m_ScrollArea.Content.MeasuredSize;

			// If auto hide flag set and one of measure results is larger than the scroll area, we need a scroll bar and re-measure scroll area
			if ( (!needScrollH && contentSize.Width > scrollAreaSize.Width) || (!needScrollV && contentSize.Height > scrollAreaSize.Height) )
			{
				needScrollH |= m_CanScrollH && contentSize.Width > scrollAreaSize.Width;
				needScrollV |= m_CanScrollV && contentSize.Height > scrollAreaSize.Height;

				if (needScrollH)
				{
					m_HorizontalScrollBar.Measure(scrollAreaSize);
					scrollAreaSize.Height = innerSize.Height - m_HorizontalScrollBar.MeasuredSize.Height;
				}
				if (needScrollV)
				{
					m_VerticalScrollBar.Measure(scrollAreaSize);
					scrollAreaSize.Width = innerSize.Width - m_VerticalScrollBar.MeasuredSize.Width;
					// Re-measure horizontal scroll bar to take into account the width of the vertical scroll bar
					if (needScrollH)
					{
						m_HorizontalScrollBar.Measure(scrollAreaSize);
						scrollAreaSize.Height = innerSize.Height - m_HorizontalScrollBar.MeasuredSize.Height;
					}
				}

				m_ScrollArea.Measure(scrollAreaSize);
				contentSize = m_ScrollArea.Content.MeasuredSize;

				// If setting one of the scroll bars visible caused the scroll area to shrink smaller than the content, we need one more measure pass
				if ( (!needScrollH && contentSize.Width > scrollAreaSize.Width) || (!needScrollV && contentSize.Height > scrollAreaSize.Height) )
				{
					needScrollH |= m_CanScrollH && contentSize.Width > scrollAreaSize.Width;
					needScrollV |= m_CanScrollV && contentSize.Height > scrollAreaSize.Height;

					if (needScrollH)
					{
						m_HorizontalScrollBar.Measure(scrollAreaSize);
						scrollAreaSize.Height = innerSize.Height - m_HorizontalScrollBar.MeasuredSize.Height;
					}
					if (needScrollV)
					{
						m_VerticalScrollBar.Measure(scrollAreaSize);
						scrollAreaSize.Width = innerSize.Width - m_VerticalScrollBar.MeasuredSize.Width;
						// Re-measure horizontal scroll bar to take into account the width of the vertical scroll bar
						if (needScrollH)
						{
							m_HorizontalScrollBar.Measure(scrollAreaSize);
							scrollAreaSize.Height = innerSize.Height - m_HorizontalScrollBar.MeasuredSize.Height;
						}
					}

					m_ScrollArea.Measure(scrollAreaSize);
				}
			}

			if (needScrollH)
			{
				m_HorizontalScrollBar.IsDisabled = false;
				m_HorizontalScrollBar.Collapse(false, false);
			}
			else
			{
				//m_HorizontalScrollBar.SetScrollAmount(0, true);
				m_HorizontalScrollBar.IsDisabled = true;
				if (m_AutoHideBars)
					m_HorizontalScrollBar.Collapse(true, false);
			}
			if (needScrollV)
			{
				m_VerticalScrollBar.IsDisabled = false;
				m_VerticalScrollBar.Collapse(false, false);
			}
			else
			{
				//m_VerticalScrollBar.SetScrollAmount(0, true);
				m_VerticalScrollBar.IsDisabled = true;
				if (m_AutoHideBars)
					m_VerticalScrollBar.Collapse(true, false);
			}

			if (!m_CanScrollH || m_AutoSizeToContent)
				availableSize.Width = Math.Min(availableSize.Width, contentSize.Width + Padding.Left + Padding.Right);
			else
				availableSize.Width = m_VerticalScrollBar.Width;
			if (!m_CanScrollV || m_AutoSizeToContent)
				availableSize.Height = Math.Min(availableSize.Height, contentSize.Height + Padding.Top + Padding.Bottom);
			else
				availableSize.Height = m_HorizontalScrollBar.Height;

			return availableSize;
		}

		protected override Size OnArrange(Size finalSize)
		{
			int scrollAreaWidth = finalSize.Width - Padding.Left - Padding.Right;
			int scrollAreaHeight = finalSize.Height - Padding.Top - Padding.Bottom;

			if (!m_VerticalScrollBar.IsCollapsed && !m_HorizontalScrollBar.IsCollapsed)
			{
				m_VerticalScrollBar.Arrange(new Rectangle(finalSize.Width - Padding.Right - m_VerticalScrollBar.MeasuredSize.Width, Padding.Top, m_VerticalScrollBar.MeasuredSize.Width, m_VerticalScrollBar.MeasuredSize.Height));
				scrollAreaWidth -= m_VerticalScrollBar.MeasuredSize.Width;
				m_HorizontalScrollBar.Arrange(new Rectangle(Padding.Left, finalSize.Height - Padding.Bottom - m_HorizontalScrollBar.MeasuredSize.Height, m_HorizontalScrollBar.MeasuredSize.Width, m_HorizontalScrollBar.MeasuredSize.Height));
				scrollAreaHeight -= m_HorizontalScrollBar.MeasuredSize.Height;
			}
			else if (!m_VerticalScrollBar.IsCollapsed)
			{
				m_VerticalScrollBar.Arrange(new Rectangle(finalSize.Width - Padding.Right - m_VerticalScrollBar.MeasuredSize.Width, Padding.Top, m_VerticalScrollBar.MeasuredSize.Width, m_VerticalScrollBar.MeasuredSize.Height));
				scrollAreaWidth -= m_VerticalScrollBar.MeasuredSize.Width;
			}
			else if (!m_HorizontalScrollBar.IsCollapsed)
			{
				m_HorizontalScrollBar.Arrange(new Rectangle(Padding.Left, finalSize.Height - Padding.Bottom - m_HorizontalScrollBar.MeasuredSize.Height, m_HorizontalScrollBar.MeasuredSize.Width, m_HorizontalScrollBar.MeasuredSize.Height));
				scrollAreaHeight -= m_HorizontalScrollBar.MeasuredSize.Height;
			}

			m_ScrollArea.Arrange(new Rectangle(Padding.Left, Padding.Top, scrollAreaWidth, scrollAreaHeight));

			UpdateScrollBars();

			return finalSize;
		}

		/// <summary>
		/// Handler invoked on mouse wheel event.
		/// </summary>
		/// <param name="delta">Scroll delta.</param>
		/// <returns></returns>
		protected override bool OnMouseWheeled(int delta)
		{
			if (CanScrollV && m_VerticalScrollBar.IsVisible)
			{
				if (m_VerticalScrollBar.SetScrollAmount(m_VerticalScrollBar.ScrollAmount - m_VerticalScrollBar.NudgeAmount * (delta / 60.0f), true))
					return true;
			}

			if (CanScrollH && m_HorizontalScrollBar.IsVisible)
			{
				if (m_HorizontalScrollBar.SetScrollAmount(m_HorizontalScrollBar.ScrollAmount - m_HorizontalScrollBar.NudgeAmount * (delta / 60.0f), true))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
		}

		protected virtual void UpdateScrollBars()
		{
			if (m_ScrollArea == null)
				return;

			m_VerticalScrollBar.SetContentSize(ContentSize.Height, ViewableContentSize.Height);
			m_HorizontalScrollBar.SetContentSize(ContentSize.Width, ViewableContentSize.Width);

			UpdateScrollArea();
		}

		protected virtual void UpdateScrollArea()
		{
			if (m_ScrollArea == null)
				return;

			int newInnerPanelPosX = 0;
			int newInnerPanelPosY = 0;

			if (CanScrollV && !m_VerticalScrollBar.IsCollapsed)
			{
				newInnerPanelPosY = (int)(-(ContentSize.Height - ViewableContentSize.Height) * m_VerticalScrollBar.ScrollAmount);
			}
			if (CanScrollH && !m_HorizontalScrollBar.IsCollapsed)
			{
				newInnerPanelPosX = (int)(-(ContentSize.Width - ViewableContentSize.Width) * m_HorizontalScrollBar.ScrollAmount);
			}

			m_ScrollArea.SetScrollPosition(newInnerPanelPosX, newInnerPanelPosY);
		}

		public virtual void ScrollToTop()
		{
			if (CanScrollV)
			{
				UpdateScrollArea();
				m_VerticalScrollBar.ScrollToTop();
			}
		}

		public virtual void ScrollToBottom()
		{
			if (CanScrollV)
			{
				UpdateScrollArea();
				m_VerticalScrollBar.ScrollToBottom();
			}
		}

		public virtual void ScrollToLeft()
		{
			if (CanScrollH)
			{
				UpdateScrollArea();
				m_VerticalScrollBar.ScrollToLeft();
			}
		}

		public virtual void ScrollToRight()
		{
			if (CanScrollH)
			{
				UpdateScrollArea();
				m_VerticalScrollBar.ScrollToRight();
			}
		}

		/// <summary>
		/// Ensure that given rectangle is visible on the scroll control.
		/// </summary>
		/// <param name="rect">Rectange to make visible.</param>
		public void EnsureVisible(Rectangle rect)
		{
			EnsureVisible(rect, Size.Zero);
		}

		/// <summary>
		/// Ensure that given rectangle is visible on the scroll control. If scrolling is needed, minimum scrolling is given as a parameter.
		/// </summary>
		/// <param name="rect">Rectange to make visible.</param>
		/// <param name="minChange">Minimum scrolling if scrolling needed.</param>
		public void EnsureVisible(Rectangle rect, Size minChange)
		{
			Rectangle bounds = new Rectangle(HorizontalScroll, VerticalScroll, ViewableContentSize);

			Point offset = Point.Zero;

			if (CanScrollH)
			{
				if (rect.X < bounds.X)
					offset.X = rect.X - bounds.X;
				else if (rect.Right > bounds.Right)
					offset.X = rect.Right - bounds.Right;
			}

			if (CanScrollV)
			{
				if (rect.Y < bounds.Y)
					offset.Y = rect.Y - bounds.Y;
				else if (rect.Bottom > bounds.Bottom)
					offset.Y = rect.Bottom - bounds.Bottom;
			}

			if (offset.X < 0)
				HorizontalScroll += Math.Min(offset.X, -minChange.Width);
			else if (offset.X > 0)
				HorizontalScroll += Math.Max(offset.X, minChange.Width);

			if (offset.Y < 0)
				VerticalScroll += Math.Min(offset.Y, -minChange.Height);
			else if (offset.Y > 0)
				VerticalScroll += Math.Max(offset.Y, minChange.Height);
		}
	}
}
