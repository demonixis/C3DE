using System;
using Gwen;
using Gwen.Control;
using Gwen.Control.Property;

namespace C3DE.Editor.UI.Items
{
    public sealed class StringControl : PropertyBase
    {
        private readonly TextBox _textBox;

        public event Action<string> Changed;

        public StringControl(ControlBase parent) : base(parent)
        {
            _textBox = new TextBox(this);
            _textBox.Dock = Gwen.Dock.Fill;
            _textBox.Padding = new Gwen.Padding(0, 0, 0, 0);
            _textBox.ShouldDrawBackground = false;
            MinimumSize = new Size(120, 24);
            _textBox.TextChanged += (_, __) => Changed?.Invoke(_textBox.Text);
        }

        public void SetValue(string value)
        {
            _textBox.SetText(value ?? string.Empty, false);
        }

        public override string Value
        {
            get => _textBox.Text;
            set => base.Value = value;
        }

        public override void SetValue(string value, bool fireEvents = false)
        {
            _textBox.SetText(value ?? string.Empty, fireEvents);
        }

        public override bool IsEditing => _textBox.HasFocus;

        protected override Size OnMeasure(Size availableSize)
        {
            return new Size(Math.Max(MinimumSize.Width, availableSize.Width), MinimumSize.Height);
        }

        protected override Size OnArrange(Size finalSize)
        {
            _textBox.Arrange(new Rectangle(Point.Zero, finalSize));
            return finalSize;
        }
    }
}
