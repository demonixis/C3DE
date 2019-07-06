using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Editable ComboBox control.
	/// </summary>
	[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
	public class EditableComboBox : ComboBoxBase
	{
		private readonly TextBox m_TextBox;
		private readonly ComboBoxButton m_Button;

		/// <summary>
		/// Invoked when the text has changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> TextChanged
		{
			add
			{
				m_TextBox.TextChanged += value;
			}
			remove
			{
				m_TextBox.TextChanged -= value;
			}
		}

		/// <summary>
		/// Invoked when the submit key has been pressed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> SubmitPressed
		{
			add
			{
				m_TextBox.SubmitPressed += value;
			}
			remove
			{
				m_TextBox.SubmitPressed -= value;
			}
		}

		/// <summary>
		/// Text.
		/// </summary>
		[Xml.XmlProperty]
		public virtual string Text { get { return m_TextBox.Text; } set { m_TextBox.SetText(value); } }

		/// <summary>
		/// Text color.
		/// </summary>
		[Xml.XmlProperty]
		public Color TextColor { get { return m_TextBox.TextColor; } set { m_TextBox.TextColor = value; } }

		/// <summary>
		/// Font.
		/// </summary>
		[Xml.XmlProperty]
		public Font Font { get { return m_TextBox.Font; } set { m_TextBox.Font = value; } }

		internal bool IsDepressed { get { return m_Button.IsDepressed; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="EditableComboBox"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public EditableComboBox(ControlBase parent)
			: base(parent)
		{
			m_TextBox = new TextBox(this);

			m_Button = new ComboBoxButton(m_TextBox, this);
			m_Button.Dock = Dock.Right;
			m_Button.Clicked += OnClicked;

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
		/// Internal handler for item selected event.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected override void OnItemSelected(ControlBase control, EventArgs args)
		{
			if (!IsDisabled)
			{
				MenuItem item = control as MenuItem;
				if (null == item) return;

				m_TextBox.Text = item.Text;
			}

			base.OnItemSelected(control, args);
		}

		protected override Size OnMeasure(Size availableSize)
		{
			return m_TextBox.Measure(availableSize);
		}

		protected override Size OnArrange(Size finalSize)
		{
			m_TextBox.Arrange(new Rectangle(Point.Zero, finalSize));

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
			EditableComboBox element = new EditableComboBox(parent);
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
