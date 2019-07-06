using System;

namespace Gwen.Control.Property
{
    /// <summary>
    /// Text property.
    /// </summary>
    public class Text : PropertyBase
    {
        protected readonly TextBox m_TextBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="Text"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Text(Control.ControlBase parent)
			: base(parent)
        {
            m_TextBox = new TextBox(this);
			m_TextBox.Dock = Dock.Fill;
			m_TextBox.Padding = Padding.Zero;
            m_TextBox.ShouldDrawBackground = false;
            m_TextBox.TextChanged += OnValueChanged;
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
            get { return m_TextBox.HasFocus; }
        }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get { return base.IsHovered | m_TextBox.IsHovered; }
        }

		/*
		protected override Size Measure(Size availableSize)
		{
			return m_TextBox.DoMeasure(availableSize);
		}

		protected override Size Arrange(Size finalSize)
		{
			m_TextBox.DoArrange(new Rectangle(0, 0, finalSize.Width, m_TextBox.MeasuredSize.Height));

			return new Size(finalSize.Width, m_TextBox.MeasuredSize.Height);
		}
		*/
	}
}
