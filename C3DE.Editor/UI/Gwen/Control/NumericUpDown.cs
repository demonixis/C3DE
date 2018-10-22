using System;
using Gwen.Control.Layout;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Numeric up/down.
	/// </summary>
	[Xml.XmlControl]
	public class NumericUpDown : TextBoxNumeric
    {
        private float m_Max;
		private float m_Min;
		private float m_Step;

        private readonly Splitter m_Splitter;
        private readonly UpDownButton_Up m_Up;
        private readonly UpDownButton_Down m_Down;

		/// <summary>
		/// Minimum value.
		/// </summary>
		[Xml.XmlProperty]
		public float Min { get { return m_Min; } set { m_Min = value; } }

		/// <summary>
		/// Maximum value.
		/// </summary>
		[Xml.XmlProperty]
		public float Max { get { return m_Max; } set { m_Max = value; } }

		[Xml.XmlProperty]
		public float Step { get { return m_Step; } set { m_Step = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericUpDown"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public NumericUpDown(ControlBase parent)
            : base(parent)
        {
            m_Splitter = new Splitter(this);
			m_Splitter.Dock = Dock.Right;

            m_Up = new UpDownButton_Up(m_Splitter);
            m_Up.Clicked += OnButtonUp;
            m_Up.IsTabable = false;
            m_Splitter.SetPanel(0, m_Up, false);

            m_Down = new UpDownButton_Down(m_Splitter);
            m_Down.Clicked += OnButtonDown;
            m_Down.IsTabable = false;
            m_Down.Padding = new Padding(0, 1, 1, 0);
            m_Splitter.SetPanel(1, m_Down, false);

            m_Max = 100f;
            m_Min = 0f;
            m_Value = 0f;
			m_Step = 1f;

            Text = "0";
        }

		/// <summary>
		/// Invoked when the value has been changed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Handler for Up Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyUp(bool down)
        {
            if (down) OnButtonUp(null, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Handler for Down Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool OnKeyDown(bool down)
        {
            if (down) OnButtonDown(null, new ClickedEventArgs(0, 0, true));
            return true;
        }

        /// <summary>
        /// Handler for the button up event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void OnButtonUp(ControlBase control, EventArgs args)
        {
            Value = m_Value + m_Step;
        }

        /// <summary>
        /// Handler for the button down event.
        /// </summary>
        /// <param name="control">Event source.</param>
        protected virtual void OnButtonDown(ControlBase control, ClickedEventArgs args)
        {
			Value = m_Value - m_Step;
        }

        /// <summary>
        /// Determines whether the text can be assighed to the control.
        /// </summary>
        /// <param name="str">Text to evaluate.</param>
        /// <returns>True if the text is allowed.</returns>
        protected override bool IsTextAllowed(string str)
        {
            float d;
            if (!float.TryParse(str, out d))
                return false;
            if (d < m_Min) return false;
            if (d > m_Max) return false;
            return true;
        }

		/// <summary>
		/// Numeric value of the control.
		/// </summary>
		[Xml.XmlProperty]
		public override float Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (value < m_Min) value = m_Min;
                if (value > m_Max) value = m_Max;
                if (value == m_Value) return;

                base.Value = value;
            }
        }

        /// <summary>
        /// Handler for the text changed event.
        /// </summary>
        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            if (ValueChanged != null)
                ValueChanged.Invoke(this, EventArgs.Empty);
        }

		public override void SetValue(float value, bool doEvents = true)
		{
			if (value < m_Min) value = m_Min;
			if (value > m_Max) value = m_Max;
			if (value == m_Value) return;

			base.SetValue(value, doEvents);
		}
	}
}
