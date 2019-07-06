using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Group box (container).
	/// </summary>
	[Xml.XmlControl]
	public class GroupBox : ContentControl
    {
		private readonly Text m_Text;

		/// <summary>
		/// Text.
		/// </summary>
		[Xml.XmlProperty]
		public virtual string Text { get { return m_Text.String; } set { m_Text.String = value; } }

		[Xml.XmlProperty]
		public override Padding Padding { get { return m_InnerPanel.Padding; } set { m_InnerPanel.Padding = value; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupBox"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public GroupBox(ControlBase parent)
			: base(parent)
        {
			m_Text = new Text(this);

			m_InnerPanel = new InnerContentControl(this);
        }

		protected override Size OnMeasure(Size availableSize)
		{
			Size titleSize = m_Text.Measure(availableSize);

			Size innerSize = Size.Zero;
			if (m_InnerPanel != null)
				innerSize = m_InnerPanel.Measure(new Size(availableSize.Width - 5 - 5, availableSize.Height - titleSize.Height - 5));

			return new Size(Math.Max(10 + titleSize.Width + 10, 5 + innerSize.Width + 5), titleSize.Height + innerSize.Height + 5);
		}

		protected override Size OnArrange(Size finalSize)
		{
			Size size = finalSize;

			m_Text.Arrange(new Rectangle(10, 0, m_Text.MeasuredSize.Width, m_Text.MeasuredSize.Height));

			if (m_InnerPanel != null)
				m_InnerPanel.Arrange(new Rectangle(5, m_Text.MeasuredSize.Height, finalSize.Width - 5 - 5, finalSize.Height - m_Text.MeasuredSize.Height - 5));

			return size;
		}

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            skin.DrawGroupBox(this, 10, m_Text.ActualHeight, m_Text.ActualWidth);
        }
	}
}
