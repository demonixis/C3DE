using System;

namespace Gwen.Control.Property
{
    /// <summary>
    /// Base control for property entry.
    /// </summary>
    public class PropertyBase : Control.ControlBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBase"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyBase(Control.ControlBase parent) : base(parent)
        {
        }

        /// <summary>
        /// Invoked when the property value has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Property value (todo: always string, which is ugly. do something about it).
        /// </summary>
        public virtual string Value { get { return null; } set { SetValue(value, false); } }

        /// <summary>
        /// Indicates whether the property value is being edited.
        /// </summary>
        public virtual bool IsEditing { get { return false; } }

        protected virtual void DoChanged()
        {
            if (ValueChanged != null)
                ValueChanged.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnValueChanged(Control.ControlBase control, EventArgs args)
        {
            DoChanged();
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="value">Value to set.</param>
        /// <param name="fireEvents">Determines whether to fire "value changed" event.</param>
        public virtual void SetValue(string value, bool fireEvents = false)
        {
            
        }
    }
}
