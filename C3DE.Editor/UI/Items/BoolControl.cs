using System;
using Gwen;
using Gwen.Control;
using Gwen.Control.Property;

namespace C3DE.Editor.UI.Items
{
    public sealed class BoolControl : PropertyBase
    {
        private readonly CheckBox _checkBox;

        public event Action<bool> Changed;

        public BoolControl(ControlBase parent) : base(parent)
        {
            MinimumSize = new Size(40, 18);
            _checkBox = new CheckBox(this)
            {
                Dock = Gwen.Dock.Left,
                ShouldDrawBackground = false
            };
            _checkBox.CheckChanged += (_, __) => Changed?.Invoke(_checkBox.IsChecked);
        }

        public void SetBool(bool value)
        {
            _checkBox.IsChecked = value;
        }

        public override string Value
        {
            get => _checkBox.IsChecked ? "true" : "false";
            set => base.Value = value;
        }

        public override void SetValue(string value, bool fireEvents = false)
        {
            _checkBox.IsChecked = value == "1" || value == "true";
        }

        public override bool IsEditing => _checkBox.HasFocus;

        protected override Size OnMeasure(Size availableSize)
        {
            return new Size(Math.Max(MinimumSize.Width, 18), MinimumSize.Height);
        }

        protected override Size OnArrange(Size finalSize)
        {
            _checkBox.Arrange(new Rectangle(0, (finalSize.Height - 15) / 2, 15, 15));
            return finalSize;
        }
    }
}
