using Gwen;
using Gwen.Control;
using Gwen.Control.Layout;
using Gwen.Control.Property;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Editor.UI.Items
{
    public class Vector3Control : PropertyBase
    {
        private NumericUpDown _X;
        private NumericUpDown _Y;
        private NumericUpDown _Z;

        public event Action<Vector3Control, float, float, float> Vector3Changed = null;

        public Vector3Control(ControlBase parent) : base(parent)
        {
            var layout = new FlowLayout(this);
            layout.Dock = Dock.Top;
            layout.MinimumSize = new Size(300, 30);

            _X = AddControl(layout, "X");
            _Y = AddControl(layout, "Y");
            _Z = AddControl(layout, "Z");
        }

        private NumericUpDown AddControl(ControlBase parent, string text)
        {
            var label = new Label(parent);
            label.Width = 10;
            label.Text = $"{text}:";
            label.VerticalAlignment = VerticalAlignment.Center;

            var num = new NumericUpDown(parent);
            num.Width = 35;
            num.ValueChanged += OnUpDownChanged;
            num.UserData = text;
            num.Value = 0;
            num.Margin = Margin.Five;
            num.Step = 0.1f;

            return num;
        }

        public void SetVector(Vector3 vector)
        {
            _X.Value = vector.X;
            _Y.Value = vector.Y;
            _Z.Value = vector.Z;
        }

        private void OnUpDownChanged(ControlBase sender, EventArgs arguments)
        {
            Vector3Changed?.Invoke(this, _X.Value, _Y.Value, _Z.Value);
        }
    }
}
