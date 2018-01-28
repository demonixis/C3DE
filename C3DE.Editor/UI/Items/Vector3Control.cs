using Gwen;
using Gwen.Control;
using Gwen.Control.Layout;
using Gwen.Control.Property;
using System;

namespace C3DE.Editor.UI.Items
{
    public class Vector3Control : PropertyBase
    {
        public event Action<Vector3Control, string, float> ValueChanged = null;

        public Vector3Control(ControlBase parent) : base(parent)
        {
            var layout = new FlowLayout(this);
            layout.Dock = Dock.Top;
            layout.MinimumSize = new Size(300, 30);

            AddControl(layout, "X");
            AddControl(layout, "Y");
            AddControl(layout, "Z");
        }

        private void AddControl(ControlBase parent, string text)
        {
            var label = new Label(parent);
            label.Width = 10;
            label.Text = $"{text}:";
            label.VerticalAlignment = VerticalAlignment.Center;

            var num = new NumericUpDown(parent);
            num.Width = 35;
            num.ValueChanged += OnValueChanged;
            num.UserData = text;
            num.Value = 0;
            num.Margin = Margin.Five;
        }

        private void OnValueChanged(ControlBase sender, System.EventArgs arguments)
        {
            var num = (NumericUpDown)sender;
            ValueChanged?.Invoke(this, num.UserData.ToString(), num.Value);
        }
    }
}
