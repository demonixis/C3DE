using Gwen;
using Gwen.Control;
using System;

namespace C3DE.Editor.UI.Items
{
    public class TransformControl : ControlBase
    {
        public event Action<string, string, float> ValueChanged = null;

        public TransformControl(ControlBase parent = null) 
            : base(parent)
        {
            var tree = new PropertyTree(this);
            tree.Dock = Dock.Top;
            tree.Width = 300;
            tree.AutoSizeToContent = true;

            var translation = tree.Add("Transform");
            AddControl(translation, "Translation");
            AddControl(translation, "Rotation");
            AddControl(translation, "Scale");
        }

        private void AddControl(Properties parent, string label)
        {
            var control = new Vector3Control(parent);
            control.ValueChanged += Control_ValueChanged;
            control.UserData = label;
           
            parent.Add(label, control);
        }

        private void Control_ValueChanged(Vector3Control control, string label, float value)
        {
            ValueChanged?.Invoke($"{control.UserData}", $"{label}", value);
        }
    }
}
