using System;
using Gwen.Input;

namespace Gwen.Control
{
	/// <summary>
	/// RadioButton with label.
	/// </summary>
	[Xml.XmlControl]
	public class LabeledRadioButton : ControlBase
	{
		private readonly RadioButton m_RadioButton;
		private readonly Label m_Label;

		/// <summary>
		/// Label text.
		/// </summary>
		[Xml.XmlProperty]
		public string Text { get { return m_Label.Text; } set { m_Label.Text = value; } }

		/// <summary>
		/// Invoked when the radiobutton has been checked.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> Checked
		{
			add
			{
				m_RadioButton.Checked += value;
			}
			remove
			{
				m_RadioButton.Checked -= value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LabeledRadioButton"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public LabeledRadioButton(ControlBase parent)
			: base(parent)
		{
			MouseInputEnabled = true;

			m_RadioButton = new RadioButton(this);
			m_RadioButton.IsTabable = false;
			m_RadioButton.KeyboardInputEnabled = false;

			m_Label = new Label(this);
			m_Label.Alignment = Alignment.CenterV | Alignment.Left;
			m_Label.Text = "Radio Button";
			m_Label.Clicked += delegate(ControlBase control, ClickedEventArgs args) { m_RadioButton.Press(control); };
			m_Label.IsTabable = false;
			m_Label.KeyboardInputEnabled = false;
		}

		protected override Size OnMeasure(Size availableSize)
		{
			Size labelSize = m_Label.Measure(availableSize);
			Size radioButtonSize = m_RadioButton.Measure(availableSize);

			return new Size(labelSize.Width + 4 + radioButtonSize.Width, Math.Max(labelSize.Height, radioButtonSize.Height));
		}

		protected override Size OnArrange(Size finalSize)
		{
			if (m_RadioButton.MeasuredSize.Height > m_Label.MeasuredSize.Height)
			{
				m_RadioButton.Arrange(new Rectangle(0, 0, m_RadioButton.MeasuredSize.Width, m_RadioButton.MeasuredSize.Height));
				m_Label.Arrange(new Rectangle(m_RadioButton.MeasuredSize.Width + 4, (m_RadioButton.MeasuredSize.Height - m_Label.MeasuredSize.Height) / 2, m_Label.MeasuredSize.Width, m_Label.MeasuredSize.Height));
			}
			else
			{
				m_RadioButton.Arrange(new Rectangle(0, (m_Label.MeasuredSize.Height - m_RadioButton.MeasuredSize.Height) / 2, m_RadioButton.MeasuredSize.Width, m_RadioButton.MeasuredSize.Height));
				m_Label.Arrange(new Rectangle(m_RadioButton.MeasuredSize.Width + 4, 0, m_Label.MeasuredSize.Width, m_Label.MeasuredSize.Height));
			}

			return finalSize;
		}

		/// <summary>
		/// Renders the focus overlay.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void RenderFocus(Skin.SkinBase skin)
		{
			if (InputHandler.KeyboardFocus != this) return;
			if (!IsTabable) return;

			skin.DrawKeyboardHighlight(this, RenderBounds, 0);
		}

		// todo: would be nice to remove that
		internal RadioButton RadioButton { get { return m_RadioButton; } }

		/// <summary>
		/// Handler for Space keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeySpace(bool down)
		{
			if (down)
				m_RadioButton.IsChecked = !m_RadioButton.IsChecked;
			return true;
		}

		/// <summary>
		/// Selects the radio button.
		/// </summary>
		public virtual void Select()
		{
			m_RadioButton.IsChecked = true;
		}
	}
}
