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
        public Component Component { get; protected set; }
        public ComponentChangeType ChangeType { get; protected set; }

        public ComponentChangedEventArgs(Component component, ComponentChangeType changeType)
        {
            Component = component;
            ChangeType = changeType;
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
