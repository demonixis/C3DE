using System;
using System.Linq;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	[Xml.XmlControl]
	public class ToolWindow : WindowBase
	{
		private bool m_vertical;

		[Xml.XmlProperty]
		public bool Vertical
		{
			get
			{
				return m_vertical;
			}
			set
			{
				m_vertical = value;
				if (m_vertical)
				{
					m_DragBar.Height = BaseUnit + 2;
					m_DragBar.Width = Util.Ignore;
				}
				else
				{
					m_DragBar.Width = BaseUnit + 2;
					m_DragBar.Height = Util.Ignore;
				}
				EnableResizing();
				Invalidate();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ToolWindow"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ToolWindow(ControlBase parent)
			: base(parent)
		{
			m_DragBar = new Dragger(this);
			m_DragBar.Target = this;
			m_DragBar.SendToBack();

			Vertical = false;

			m_InnerPanel = new InnerContentControl(this);
			m_InnerPanel.SendToBack();
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			skin.DrawToolWindow(this, m_vertical, m_vertical ? m_DragBar.ActualHeight : m_DragBar.ActualWidth);
		}

		/// <summary>
		/// Renders under the actual control (shadows etc).
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void RenderUnder(Skin.SkinBase skin)
		{
			base.RenderUnder(skin);
			skin.DrawShadow(this);
		}

		/// <summary>
		/// Renders the focus overlay.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void RenderFocus(Skin.SkinBase skin)
		{

		}

		protected override Size OnMeasure(Size availableSize)
		{
			Size titleBarSize = m_DragBar.Measure(new Size(availableSize.Width, availableSize.Height));

			if (m_InnerPanel != null)
			{
				if (m_vertical)
					m_InnerPanel.Measure(new Size(availableSize.Width, availableSize.Height - titleBarSize.Height));
				else
					m_InnerPanel.Measure(new Size(availableSize.Width - titleBarSize.Width, availableSize.Height));
			}

			if (m_vertical)
				return base.OnMeasure(new Size(m_InnerPanel.MeasuredSize.Width, m_InnerPanel.MeasuredSize.Height + titleBarSize.Height));
			else
				return base.OnMeasure(new Size(m_InnerPanel.MeasuredSize.Width + titleBarSize.Width, m_InnerPanel.MeasuredSize.Height));
		}

		protected override Size OnArrange(Size finalSize)
		{
			if (m_vertical)
				m_DragBar.Arrange(new Rectangle(0, 0, finalSize.Width, m_DragBar.MeasuredSize.Height));
			else
				m_DragBar.Arrange(new Rectangle(0, 0, m_DragBar.MeasuredSize.Width, finalSize.Height));

			if (m_InnerPanel != null)
			{
				if (m_vertical)
					m_InnerPanel.Arrange(new Rectangle(0, m_DragBar.MeasuredSize.Height, finalSize.Width, finalSize.Height - m_DragBar.MeasuredSize.Height));
				else
					m_InnerPanel.Arrange(new Rectangle(m_DragBar.MeasuredSize.Width, 0, finalSize.Width - m_DragBar.MeasuredSize.Width, finalSize.Height));
			}

			return base.OnArrange(finalSize);
		}

		public override void EnableResizing(bool left = true, bool top = true, bool right = true, bool bottom = true)
		{
			base.EnableResizing(!m_vertical ? false : left, m_vertical ? false : top, right, bottom);
		}
	}
}
