using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// ComboBox control.
	/// </summary>
	[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
	public class ComboBox : ComboBoxBase
	{
		private readonly Button m_Button;
		private readonly DownArrow m_DownArrow;

		internal bool IsDepressed { get { return m_Button.IsDepressed; } }
		public override bool IsHovered { get { return m_Button.IsHovered; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="ComboBox"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ComboBox(ControlBase parent)
			: base(parent)
		{
			m_Button = new Button(this);
			m_Button.Alignment = Alignment.Left | Alignment.CenterV;
			m_Button.Text = String.Empty;
			m_Button.TextPadding = Padding.Three;
			m_Button.Clicked += OnClicked;

			m_DownArrow = new DownArrow(this);

			IsTabable = true;
			KeyboardInputEnabled = true;
		}

		/// <summary>
		/// Internal Pressed implementation.
		/// </summary>
		private void OnClicked(ControlBase sender, ClickedEventArgs args)
		{
			if (IsOpen)
			{
				Close();
			}
			else
			{
				Open();
			}
		}

		/// <summary>
		/// Removes all items.
		/// </summary>
		public override void RemoveAll()
		{
			m_Button.Text = String.Empty;
			base.RemoveAll();
		}

		/// <summary>
		/// Internal handler for item selected event.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected override void OnItemSelected(ControlBase control, EventArgs args)
		{
			if (!IsDisabled)
			{
				MenuItem item = control as MenuItem;
				if (null == item) return;

				m_Button.Text = item.Text;
			}

			base.OnItemSelected(control, args);
		}

		protected override Size OnMeasure(Size availableSize)
		{
			return Size.Max(m_Button.Measure(availableSize), m_DownArrow.Measure(availableSize));
		}

		protected override Size OnArrange(Size finalSize)
		{
			m_Button.Arrange(new Rectangle(Point.Zero, finalSize));

			m_DownArrow.Arrange(new Rectangle(finalSize.Width - m_Button.TextPadding.Right - m_DownArrow.MeasuredSize.Width, (finalSize.Height - m_DownArrow.MeasuredSize.Height) / 2, m_DownArrow.MeasuredSize.Width, m_DownArrow.MeasuredSize.Height));

			return finalSize;
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			skin.DrawComboBox(this, m_Button.IsDepressed, IsOpen);
		}

		/// <summary>
		/// Renders the focus overlay.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void RenderFocus(Skin.SkinBase skin)
		{

		}

		internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent)
		{
			ComboBox element = new ComboBox(parent);
			parser.ParseAttributes(element);
			if (parser.MoveToContent())
			{
				foreach (string elementName in parser.NextElement())
				{
					if (elementName == "Option")
					{
						element.AddItem(parser.ParseElement<MenuItem>(element));
					}
				}
			}
			return element;
		}
	}
}
