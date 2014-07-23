using C3DE.Components;
using System;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// A scene object is the base object on the scene.
    /// </summary>
    public class SceneObject
    {
        private enum SOFlags
        {
            RemoveComponent = 0x10
        }

        protected Transform transform;
        protected bool enabled;
        protected bool isStatic;
        protected List<Component> components;
        protected uint _dirtyFlags;

        public bool IsStatic
        {
            get { return isStatic; }
            set { isStatic = value; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (value != enabled)
                {
                    NotifyPropertyChanged("Enabled");
                    enabled = value;
                }
            }
        }

        public Transform Transform
        {
            get { return transform; }
            protected set { transform = value; }
        }

        #region Events

        /// <summary>
        /// Called when a component is added or removed.
        /// </summary>
        public event EventHandler<ComponentChangedEventArgs> ComponentChanged = null;

        /// <summary>
        /// Called when a registered property has changed.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged = null;

        private void NotifyComponentChanged(Component component, bool added = true)
        {
            if (ComponentChanged != null)
                ComponentChanged(this, new ComponentChangedEventArgs(component, added));
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        public SceneObject()
        {
            transform = new Transform(this);

            components = new List<Component>();
            components.Add(transform);

            enabled = true;
            isStatic = false;
        }

        /// <summary>
        /// Add a scene object as children of this scene object. It is not possible to add the same object twice
        /// </summary>
        /// <param name="sceneObject">The scene object to add.</param>
        /// <returns>Return true if added, otherwise return false.</returns>
        public virtual bool Add(SceneObject sceneObject)
        {
            if (!transform.Transforms.Contains(sceneObject.transform))
            {
                // Add the scene object to the scene if not yet added.
                if (sceneObject.Transform.Root == null && transform.Root != transform)
                {
                    if (transform.Root != null)
                        transform.Root.SceneObject.Add(sceneObject);
                    else
                        throw new Exception("You need to attach first the main scene object to scene.");
                }

                // Remove its parent's transform
                if (sceneObject.Transform.Parent != null)
                    sceneObject.Transform.Parent.Transforms.Remove(sceneObject.Transform);

                // Add to current transform
                sceneObject.transform.Parent = transform;
                sceneObject.transform.Root = transform.Root;
                transform.Transforms.Add(sceneObject.transform);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove a scene object. Its transform parent will be the root transform.
        /// </summary>
        /// <param name="sceneObject">The scene object to remove.</param>
        /// <returns>Return true if succed, otherwise return false.</returns>
        public virtual bool Remove(SceneObject sceneObject)
        {
            if (sceneObject.transform.Parent == transform)
            {
                transform.Transforms.Remove(sceneObject.transform);
                sceneObject.transform.Parent = transform.Root;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Update scene object components
        /// </summary>
        public virtual void Update()
        {
            foreach (var component in components)
            {
                if (component.Enabled)
                    component.Update();
            }
        }

        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T();

            if (component is Transform)
            {
                component = null;
                return transform as T;
            }

            component.SceneObject = this;
            components.Add(component);
            components.Sort();

            NotifyComponentChanged(component);

            return component;
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var component in components)
            {
                if (component is T)
                    return component as T;
            }

            return null;
        }

        public bool RemoveComponent(Component component)
        {
            int index = -1;
            int size = components.Count;
            int i = 0;

            while (i < size && index == -1)
                index = (components[i] == component) ? i : index;

            if (index > -1)
            {
                components.RemoveAt(index);

                if (ComponentChanged != null)
                    ComponentChanged(this, new ComponentChangedEventArgs(component, false));
            }

            return index > -1;
        }
    }
}
