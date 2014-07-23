using C3DE.Components;
using System;

namespace C3DE
{
    public class ComponentChangedEventArgs : EventArgs
    {
        public Component Component { get; protected set; }
        public bool Added { get; protected set; }

        public ComponentChangedEventArgs(Component component, bool added)
        {
            Component = component;
            Added = added;
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
