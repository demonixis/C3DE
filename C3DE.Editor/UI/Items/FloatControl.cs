using System;
using Gwen;
using Gwen.Control;
using Gwen.Control.Property;

namespace C3DE.Editor.UI.Items
{
    public sealed class FloatControl : PropertyBase
    {
        private readonly NumericUpDown _numeric;

        public event Action<float> Changed;

        public FloatControl(ControlBase parent, float min = -100000.0f, float max = 100000.0f, float step = 0.1f) : base(parent)
        {
            MinimumSize = new Size(120, 24);
            _numeric = new NumericUpDown(this)
            {
                Dock = Gwen.Dock.Fill,
                Min = min,
                Max = max,
                Step = step
            };
            _numeric.ValueChanged += (_, __) => Changed?.Invoke(_numeric.Value);
        }

        public void SetFloat(float value)
        {
            _numeric.SetValue(value, false);
        }

        public override string Value
        {
            get => _numeric.Value.ToString();
            set => base.Value = value;
        }

        public override void SetValue(string value, bool fireEvents = false)
        {
            if (float.TryParse(value, out var result))
                _numeric.SetValue(result, fireEvents);
        }

        public override bool IsEditing => _numeric.HasFocus;

        protected override Size OnMeasure(Size availableSize)
        {
            return new Size(Math.Max(MinimumSize.Width, availableSize.Width), MinimumSize.Height);
        }

        protected override Size OnArrange(Size finalSize)
        {
            _numeric.Arrange(new Rectangle(Point.Zero, finalSize));
            return finalSize;
        }
    }
}
