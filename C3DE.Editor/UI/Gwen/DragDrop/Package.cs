using System;
using Gwen.Control;

namespace Gwen.DragDrop
{
    public class Package
    {
        public string Name;
        public object UserData;
        public bool IsDraggable;
        public ControlBase DrawControl;
        public Point HoldOffset;
    }
}
