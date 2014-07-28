using C3DE.Components;
using C3DE.Utils;
using Microsoft.Xna.Framework.Content;
using System;

namespace C3DE
{
    /// <summary>
    /// A scene object is the base object on the scene.
    /// </summary>
    public class SceneObject
    {
        #region Private/protected declarations

        private static int SceneObjectCounter = 0;

        protected Transform transform;
        protected Scene scene;
        protected bool enabled;
        protected bool isStatic;
        protected SmartList<Component> components;
        protected bool initialized;

        #endregion

        #region Fields

        public int Id { get; private set; }

        public string Name { get; protected set; }

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

        public Scene Scene
        {
            get { return scene; }
            internal set { scene = value; }
        }

        public SmartList<Component> Components
        {
            get { return components; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when a component is added or removed.
        /// </summary>
        public event EventHandler<ComponentChangedEventArgs> ComponentChanged = null;

        /// <summary>
        /// Called when a registered property has changed.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged = null;

        private void NotifyComponentChanged(Component component, ComponentChangeType type = ComponentChangeType.Add)
        {
            if (ComponentChanged != null)
                ComponentChanged(this, new ComponentChangedEventArgs(component, type));
        }

        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        /// <summary>
        /// Create a basic scene object.
        /// </summary>
        public SceneObject()
        {
            transform = new Transform(this);

            components = new SmartList<Component>();
            components.Add(transform);

            enabled = true;
            isStatic = false;

            Id = ++SceneObjectCounter;
            Name = "SceneObject_" + Id;
        }

        /// <summary>
        /// Initialize and load specific content.
        /// </summary>
        /// <param name="content">The content manager.</param>
        public virtual void LoadContent(ContentManager content)
        {
            if (!initialized)
            {
                for (int i = 0; i < components.Size; i++)
                    components[i].LoadContent(content);

                components.CheckRequired = true;
                initialized = true;
            }
        }

        /// <summary>
        /// Update components.
        /// </summary>
        public virtual void Update()
        {
            components.Check();

            for (int i = 0; i < components.Size; i++)
            {
                if (components[i].Enabled)
                    components[i].Update();
            }
        }

        #region SceneObject collection

        /// <summary>
        /// Add a scene object as children of this scene object. It is not possible to add the same object twice
        /// </summary>
        /// <param name="sceneObject">The scene object to add.</param>
        /// <returns>Return true if added, otherwise return false.</returns>
        public virtual bool Add(SceneObject sceneObject)
        {
            if (!transform.Transforms.Contains(sceneObject.transform) && sceneObject != this)
            {
                // Add the scene object to the scene if not yet added.
                if (sceneObject.Scene == null && this != scene)
                {
                    if (scene != null)
                        scene.Add(sceneObject);
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

        #endregion

        #region Component collection

        /// <summary>
        /// Add a component of the specified type. Note that you can't add another Transform component.
        /// </summary>
        /// <typeparam name="T">The component's type.</typeparam>
        /// <returns>Return true if the component has been added, otherwise return false.</returns>
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

            if (initialized)
                component.LoadContent(App.Content);

            NotifyComponentChanged(component);

            return component;
        }

        /// <summary>
        /// Get a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The component's type.</typeparam>
        /// <returns>Return the first component of this type if founded, otherwise return null.</returns>
        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < components.Size; i++)
            {
                if (components[i] is T)
                    return components[i] as T;
            }

            return null;
        }

        /// <summary>
        /// Remove the component and update the scene.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        /// <returns>Return true if the component has been removed, otherwise return false.</returns>
        public bool RemoveComponent(Component component)
        {
            int index = -1;
            int i = 0;

            while (i < components.Size && index == -1)
                index = (components[i] == component) ? i : index;

            if (index > -1)
            {
                components.Remove(component);
                NotifyComponentChanged(component, ComponentChangeType.Remove);
            }

            return index > -1;
        }

        #endregion
    }
}
