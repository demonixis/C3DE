using C3DE.Components;
using System;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// A scene object is the base object on the scene.
    /// </summary>
    public class SceneObject : ICloneable, IDisposable, ISerializable
    {
        #region Private/protected declarations

        protected Transform transform;
        protected Scene scene;
        protected bool enabled;
        protected List<Component> components;
        protected bool initialized;

        #endregion

        #region Fields

        public string Id { get; set; }

        public string Name { get; set; }

        public string Tag { get; set; }

        public bool IsStatic { get; set; }

        public bool IsPrefab { get; set; }

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

        public List<Component> Components
        {
            get { return components; }
        }

        public bool Initialized
        {
            get { return initialized; }
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
        public SceneObject(string name = "")
        {
            transform = new Transform();
            transform.SceneObject = this;

            components = new List<Component>(5);
            components.Add(transform);

            transform.Awake();

            enabled = true;
            initialized = false;
            IsStatic = false;
            IsPrefab = false;

            Id = "SO_" + Guid.NewGuid();
            Name = !string.IsNullOrEmpty(name) ? name : "SceneObject_" + Id;
        }

        /// <summary>
        /// Initialize and load specific content.
        /// </summary>
        /// <param name="content">The content manager.</param>
        public virtual void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                // Sort component now then initialize it.
                components.Sort();

                for (int i = 0; i < components.Count; i++)
                {
                    components[i].initialized = true;
                    components[i].Start();
                }
            }
        }

        /// <summary>
        /// Update components.
        /// </summary>
        public virtual void Update()
        {
            for (int i = 0; i < components.Count; i++)
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
                if (this != scene)
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

        internal Component AddComponent(Component component)
        {
            if (component is Transform)
            {
                component = null;
                return transform;
            }

            component.SceneObject = this;
            component.Awake();

            components.Add(component);

            if (initialized && !component.Initialized)
            {
                component.initialized = true;
                component.Start();

                // Sort components here only if the SceneObject is already initialized.
                components.Sort();
            }

            NotifyComponentChanged(component);

            return component;
        }

        /// <summary>
        /// Add a component of the specified type. Note that you can't add another Transform component.
        /// </summary>
        /// <typeparam name="T">The component's type.</typeparam>
        /// <returns>Return true if the component has been added, otherwise return false.</returns>
        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T();

            return (T)AddComponent(component);
        }

        /// <summary>
        /// Get a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The component's type.</typeparam>
        /// <returns>Return the first component of this type if founded, otherwise return null.</returns>
        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T)
                    return components[i] as T;
            }

            return null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            List<T> comps = new List<T>();

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T)
                    comps.Add(components[i] as T);
            }

            return comps.ToArray();
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

            while (i < components.Count && index == -1)
                index = (components[i] == component) ? i : index;

            if (index > -1)
            {
                components.Remove(component);
                NotifyComponentChanged(component, ComponentChangeType.Remove);
            }

            return index > -1;
        }

        #endregion

        public static SceneObject FindById(string id)
        {
            for (int i = 0; i < Scene.current.sceneObjects.Size; i++)
                if (Scene.current.sceneObjects[i].Id == id)
                    return Scene.current.sceneObjects[i];

            return null;
        }

        public static SceneObject[] FindSceneObjectsById(string id)
        {
            List<SceneObject> sceneObjects = new List<SceneObject>();

            for (int i = 0; i < Scene.current.sceneObjects.Size; i++)
                if (Scene.current.sceneObjects[i].Id == id)
                    sceneObjects.Add(Scene.current.sceneObjects[i]);

            return sceneObjects.ToArray();
        }

        public static T[] FindObjectsOfType<T>() where T : Component
        {
            var scene = Scene.current;
            var scripts = new List<T>();

            if (scene != null)
            {
                foreach (SceneObject so in scene.sceneObjects)
                {
                    var components = so.GetComponents<T>();
                    if (components.Length > 0)
                        scripts.AddRange(components);
                }
            }

            return scripts.ToArray();
        }

        public object Clone()
        {
            SceneObject sceneObject = new SceneObject(Name + " (Clone)");

            foreach (Component component in components)
                sceneObject.AddComponent((Component)component.Clone());

            // Fixme
            sceneObject.Transform.Position = transform.Position;
            sceneObject.Transform.Rotation = transform.Rotation;
            sceneObject.Transform.LocalScale = transform.LocalScale;
            sceneObject.Id = "SO-" + Guid.NewGuid();

            return sceneObject;
        }

        public void Dispose()
        {
            foreach (Component component in components)
                component.Dispose();
        }

        public Dictionary<string, object> Serialize()
        {
            var dico = new Dictionary<string, object>();

            dico.Add("Enabled", enabled);
            dico.Add("IsPrefab", IsPrefab);
            dico.Add("IsStatic", IsStatic);
            dico.Add("Name", Name);
            dico.Add("Id", Id);

            Dictionary<string, object>[] serComponents = new Dictionary<string, object>[components.Count];
            for (int i = 0, l = components.Count; i < l; i++)
                serComponents[i] = components[i].Serialize();

            dico.Add("Components", serComponents);

            return dico;
        }

        public void Deserialize(Dictionary<string, object> data)
        {
            enabled = (bool)data["Enabled"];
            IsStatic = (bool)data["IsStatic"];
            IsPrefab = (bool)data["IsPrefab"];
            Id = (string)data["Id"];
            Name = (string)data["Name"];

            var cpnts = data["Components"] as Dictionary<string, object>[];

            foreach (var cpn in cpnts)
            {

            }
        }
    }
}
