using System;
using Gwen.Control;

namespace Gwen.Control.Internal
{
	public class WindowTitleBar : Dragger
	{
		private readonly Label m_Title;
		private readonly CloseButton m_CloseButton;

		public Label Title { get { return m_Title; } }
		public CloseButton CloseButton { get { return m_CloseButton; } }

		public WindowTitleBar(ControlBase parent)
			: base(parent)
		{
			m_Title = new Label(this);
			m_Title.Alignment = Alignment.Left | Alignment.CenterV;

			m_CloseButton = new CloseButton(this, parent as Window);
			m_CloseButton.IsTabable = false;
			m_CloseButton.Name = "closeButton";

			Target = parent;
		}

		protected override Size OnMeasure(Size availableSize)
		{
			m_Title.Measure(availableSize);

			if (!m_CloseButton.IsCollapsed)
				m_CloseButton.Measure(availableSize);

			return availableSize;
		}

		protected override Size OnArrange(Size finalSize)
		{
			m_Title.Arrange(new Rectangle(8, 0, m_Title.MeasuredSize.Width, finalSize.Height));

			if (!m_CloseButton.IsCollapsed)
			{
				int closeButtonSize = finalSize.Height;
				m_CloseButton.Arrange(new Rectangle(finalSize.Width - 6 - closeButtonSize, 0, closeButtonSize, closeButtonSize));
			}

			return finalSize;
		}
	}
}
