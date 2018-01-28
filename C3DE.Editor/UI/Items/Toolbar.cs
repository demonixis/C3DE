using Gwen;
using Gwen.Control;

namespace C3DE.Editor.UI.Items
{
    public class Toolbar : ControlBase
    {
        public Toolbar(ControlBase parent = null) : base(parent)
        {
            var toolbar = new Gwen.Control.Layout.FlowLayout(this);
            toolbar.HorizontalAlignment = HorizontalAlignment.Left;
            toolbar.VerticalAlignment = VerticalAlignment.Top;

            var icons = new[]
            {
                "Icon_Translation", "Icon_Rotation",
                "Icon_Scale", "Icon_Precision",
                "Icon_Lock", "Icon_Grid"
            };

            var tips = new[]
            {
                "Translation", "Rotation", "Scale",
                "Precision Mode", "Lock", "Grid enabled"
            };

            var toggled = new[]
            {
                true, false, false, false, false, true
            };

            for (var i = 0; i < icons.Length; i++)
            {
                var button = new Button(toolbar);
                button.ImageName = $"Icons/{icons[i]}";
                button.Size = new Size(24, 24);
                button.Margin = Margin.Five;
                button.ToolTipText = tips[i];
                button.IsToggle = true;
                button.ToggleState = toggled[i];
            }
        }
    }
}
