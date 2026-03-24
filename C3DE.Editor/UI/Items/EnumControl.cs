using System;
using Gwen;
using Gwen.Control;
using Gwen.Control.Property;

namespace C3DE.Editor.UI.Items
{
    public sealed class EnumControl : PropertyBase
    {
        private readonly ComboBox _comboBox;

        public event Action<string> Changed;

        public EnumControl(ControlBase parent, params string[] values) : base(parent)
        {
            MinimumSize = new Size(140, 24);
            _comboBox = new ComboBox(this)
            {
                Dock = Gwen.Dock.Fill
            };

            foreach (var value in values)
                _comboBox.AddItem(value, value, value);

            _comboBox.ItemSelected += (_, args) => Changed?.Invoke((args.SelectedItem as MenuItem)?.Text ?? string.Empty);
        }

        public void SetSelected(string value)
        {
            _comboBox.SelectByText(value);
        }

        public override string Value
        {
            get => _comboBox.SelectedItem?.Text ?? string.Empty;
            set => base.Value = value;
        }

        public override void SetValue(string value, bool fireEvents = false)
        {
            _comboBox.SelectByText(value);
        }

        public override bool IsEditing => _comboBox.HasFocus;

        protected override Size OnMeasure(Size availableSize)
        {
            return new Size(Math.Max(MinimumSize.Width, availableSize.Width), MinimumSize.Height);
        }

        protected override Size OnArrange(Size finalSize)
        {
            _comboBox.Arrange(new Rectangle(Point.Zero, finalSize));
            return finalSize;
        }
    }
}
