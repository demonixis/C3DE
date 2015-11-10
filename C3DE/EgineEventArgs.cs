using C3DE.Components;
using System;

namespace C3DE
{
    public enum ComponentChangeType
    {
        Add = 0, Update, Remove
    }

    public class ComponentChangedEventArgs : EventArgs
    {
        public Component Component { get; set; }
        public string PropertyName { get; set; }
        public ComponentChangeType ChangeType { get; set; }

        public ComponentChangedEventArgs(Component component, string propertyName, ComponentChangeType changeType)
        {
            Component = component;
            ChangeType = changeType;
            PropertyName = propertyName;
        }
    }

    public class PropertyChangedEventArgs : EventArgs
    {
        public string Name { get; protected set; }

        public PropertyChangedEventArgs(string name)
        {
            Name = name;
        }
    }
}
