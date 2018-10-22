using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
    /// <summary>
    /// Single property row.
    /// </summary>
    public class PropertyRow : ControlBase
    {
        private readonly Label m_Label;
        private readonly Property.PropertyBase m_Property;
        private bool m_LastEditing;
        private bool m_LastHover;

        /// <summary>
        /// Invoked when the property value has changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public bool IsEditing { get { return m_Property != null && m_Property.IsEditing; } }

        /// <summary>
        /// Property value.
        /// </summary>
        public string Value { get { return m_Property.Value; } set { m_Property.Value = value; } }

        /// <summary>
        /// Indicates whether the control is hovered by mouse pointer.
        /// </summary>
        public override bool IsHovered
        {
            get
            {
                return base.IsHovered || (m_Property != null && m_Property.IsHovered);
            }
        }

        /// <summary>
        /// Property name.
        /// </summary>
        public string Label { get { return m_Label.Text; } set { m_Label.Text = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRow"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="prop">Property control associated with this row.</param>
        public PropertyRow(ControlBase parent, Property.PropertyBase prop)
            : base(parent)
        {
			Padding = new Padding(2, 2, 2, 2);

			m_Label = new PropertyRowLabel(this);
			m_Label.Alignment = Alignment.Left | Alignment.Top;

            m_Property = prop;
            m_Property.Parent = this;
            m_Property.ValueChanged += OnValueChanged;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            /* SORRY */
            if (IsEditing != m_LastEditing)
            {
                OnEditingChanged();
                m_LastEditing = IsEditing;
            }

            if (IsHovered != m_LastHover)
            {
                OnHoverChanged();
                m_LastHover = IsHovered;
            }
            /* SORRY */

            skin.DrawPropertyRow(this, m_Label.ActualRight, IsEditing, IsHovered | m_Property.IsHovered);
        }

		protected override Size OnMeasure(Size availableSize)
		{
			Properties parent = Parent as Properties;
			if (parent != null)
			{
				Size labelSize = m_Label.Measure(new Size(parent.LabelWidth - Padding.Left - Padding.Right, availableSize.Height)) + Padding;
				Size propertySize = m_Property.Measure(new Size(availableSize.Width - parent.LabelWidth, availableSize.Height)) + Padding;

				return new Size(labelSize.Width + propertySize.Width, Math.Max(labelSize.Height, propertySize.Height));
			}

			return Size.Zero;
		}

		protected override Size OnArrange(Size finalSize)
		{
			Properties parent = Parent as Properties;
			if (parent != null)
			{
				m_Label.Arrange(new Rectangle(Padding.Left, Padding.Top, parent.LabelWidth - Padding.Left - Padding.Right, m_Label.MeasuredSize.Height));
				m_Property.Arrange(new Rectangle(parent.LabelWidth + Padding.Left, Padding.Top, finalSize.Width - parent.LabelWidth - Padding.Left - Padding.Right, m_Property.MeasuredSize.Height));

				return new Size(finalSize.Width, Math.Max(m_Label.MeasuredSize.Height, m_Property.MeasuredSize.Height) + Padding.Top + Padding.Bottom);
			}

			return Size.Zero;
		}

		protected virtual void OnValueChanged(ControlBase control, EventArgs args)
        {
            if (ValueChanged != null)
				ValueChanged.Invoke(this, EventArgs.Empty);
        }

        private void OnEditingChanged()
        {
            m_Label.Redraw();
        }

        private void OnHoverChanged()
        {
            m_Label.Redraw();
        }
    }
}
