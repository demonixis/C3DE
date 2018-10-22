using System;
using Gwen.Control.Internal;
using Gwen.Input;

namespace Gwen.Control.Property
{
    /// <summary>
    /// Color property.
    /// </summary>
    public class Color : Text
    {
        protected readonly ColorButton m_Button;

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Color(Control.ControlBase parent) : base(parent)
        {
            m_Button = new ColorButton(m_TextBox);
			m_Button.Dock = Dock.Right;
			m_Button.Width = 20;
            m_Button.Margin = new Margin(1, 1, 1, 2);
            m_Button.Clicked += OnButtonPressed;
        }

        /// <summary>
        /// Color-select button press handler.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void OnButtonPressed(Control.ControlBase control, EventArgs args)
        {
			Canvas canvas = GetCanvas();

			canvas.CloseMenus();

			Popup popup = new Popup(canvas);
			popup.DeleteOnClose = true;
			popup.IsHidden = false;
			popup.BringToFront();

			HSVColorPicker picker = new HSVColorPicker(popup);
            picker.SetColor(GetColorFromText(), false, true);
            picker.ColorChanged += OnColorChanged;

			Point p = m_Button.LocalPosToCanvas(Point.Zero);

			popup.Measure(canvas.ActualSize);
			popup.Arrange(new Rectangle(p.X + m_Button.ActualWidth - popup.MeasuredSize.Width, p.Y + ActualHeight, popup.MeasuredSize.Width, popup.MeasuredSize.Height));

			popup.Open(new Point(p.X + m_Button.ActualWidth - popup.MeasuredSize.Width, p.Y + ActualHeight));
		}

        /// <summary>
        /// Color changed handler.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void OnColorChanged(Control.ControlBase control, EventArgs args)
        {
            HSVColorPicker picker = control as HSVColorPicker;
            SetTextFromColor(picker.SelectedColor);
            DoChanged();
        }

        /// <summary>
        /// Property value.
        /// </summary>
        public override string Value
        {
            get { return m_TextBox.Text; }
            set { base.Value = value; }
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public override void SetValue(string value, bool fireEvents = false)
        {
            m_TextBox.SetText(value, fireEvents);
        }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public override bool IsEditing
        {
            get { return m_TextBox == InputHandler.KeyboardFocus; }
        }

        private void SetTextFromColor(Gwen.Color color)
        {
            m_TextBox.Text = String.Format("{0} {1} {2}", color.R, color.G, color.B);
        }

        private Gwen.Color GetColorFromText()
        {
            string[] split = m_TextBox.Text.Split(' ');

            byte red = 0;
            byte green = 0;
            byte blue = 0;
            byte alpha = 255;

            if (split.Length > 0 && split[0].Length > 0)
            {
                Byte.TryParse(split[0], out red);
            }

            if (split.Length > 1 && split[1].Length > 0)
            {
                Byte.TryParse(split[1], out green);
            }

            if (split.Length > 2 && split[2].Length > 0)
            {
                Byte.TryParse(split[2], out blue);
            }

            return new Gwen.Color(alpha, red, green, blue);
        }

        protected override void DoChanged()
        {
            base.DoChanged();
            m_Button.Color = GetColorFromText();
        }
	}
}
