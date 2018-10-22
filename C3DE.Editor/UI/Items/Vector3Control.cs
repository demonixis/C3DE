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
        private NumericUpDown m_X;
        private NumericUpDown m_Y;
        private NumericUpDown m_Z;

        public event Action<Vector3Control, float, float, float> Vector3Changed = null;

        public Vector3Control(ControlBase parent) : base(parent)
        {
            var layout = new FlowLayout(this);
            layout.Dock = Dock.Top;
            layout.MinimumSize = new Size(300, 30);

            m_X = AddControl(layout, "X");
            m_Y = AddControl(layout, "Y");
            m_Z = AddControl(layout, "Z");
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
            m_X.Value = vector.X;
            m_Y.Value = vector.Y;
            m_Z.Value = vector.Z;
        }

        private void OnUpDownChanged(ControlBase sender, EventArgs arguments)
        {
            Vector3Changed?.Invoke(this, m_X.Value, m_Y.Value, m_Z.Value);
        }
    }
}
