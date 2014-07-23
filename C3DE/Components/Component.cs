using Microsoft.Xna.Framework.Content;
using System;

namespace C3DE.Components
{
    /// <summary>
    /// A component is a part of a scene object.
    /// </summary>
    public abstract class Component : IComparable
    {
        protected bool enabled = true;
        protected int order = 1;
        protected SceneObject sceneObject;

        /// <summary>
        /// Determine if the component is enabled of disabled.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set 
            {
                if (value != enabled)
                {
                    enabled = value;
                    NotifyPropertyChanged("Enabled");
                }
            }
        }

        /// <summary>
        /// Gets the scene object of this component.
        /// </summary>
        public SceneObject SceneObject
        {
            get { return sceneObject; }
            internal set { sceneObject = value; }
        }

        public int Order
        {
            get { return order; }
            protected set 
            {
                if (value != order)
                {
                    order = value;
                    NotifyPropertyChanged("Order");
                }
            }
        }

        public event EventHandler<PropertyChangedEventArgs> PropertyChanged = null;

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Create an empty component.
        /// </summary>
        public Component()
        {
        }

        public Component(SceneObject sceneObj)
            : this()
        {
            sceneObject = sceneObj;
        }

        public virtual void LoadContent(ContentManager content)
        {
        }

        /// <summary>
        /// Update the logic.
        /// </summary>
        public virtual void Update()
        {
        }


        public int CompareTo(object obj)
        {
            var component = obj as Component;

            if (obj == null)
                return 1;

            if (order == component.order)
                return 0;
            else if (order > component.order)
                return 1;
            else
                return -1;
        }

        public T GetComponent<T>() where T : Component
        {
            return sceneObject.GetComponent<T>();
        }
    }
}