using C3DE.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace C3DE
{
    /// <summary>
    /// A scene object is the base object on the scene.
    /// </summary>
    [DataContract]
    public class GameObject : ICloneable, IDisposable
    {
        #region Private/protected declarations

        protected Transform m_Transform;
        protected Scene m_Scene;
        protected bool m_Enabled;

        [DataMember]
        protected List<Component> m_Components;

        [DataMember]
        protected bool m_Initialized;

        #endregion

        #region Fields

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Tag { get; set; }

        [DataMember]
        public bool IsStatic { get; set; }

        [DataMember]
        public bool IsPrefab { get; set; }

        [DataMember]
        public bool Enabled
        {
            get { return m_Enabled; }
            set
            {
                if (value != m_Enabled)
                {
                    m_Enabled = value;

                    NotifyPropertyChanged("Enabled");

                    if (m_Transform != null)
                    {
                        for (int i = 0, l = m_Transform.Transforms.Count; i < l; i++)
                            m_Transform.Transforms[i].GameObject.Enabled = value;
                    }
                }
            }
        }

        public Transform Transform
        {
            get { return m_Transform; }
            protected set { m_Transform = value; }
        }

        public Scene Scene
        {
            get { return m_Scene; }
            internal set { m_Scene = value; }
        }

        public List<Component> Components
        {
            get { return m_Components; }
        }

        public bool Initialized
        {
            get { return m_Initialized; }
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

        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void NotifyComponentChanged(Component component, string propertyName, ComponentChangeType type)
        {
            ComponentChanged?.Invoke(this, new ComponentChangedEventArgs(component, propertyName, type));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a basic scene object.
        /// </summary>
        public GameObject()
        {
            InternalConstructor();
        }

        public GameObject(string name)
        {
            InternalConstructor(name);
        }

        private void InternalConstructor(string name = "")
        {
            if (m_Transform == null)
            {
                m_Components = new List<Component>(5);

                m_Transform = new Transform();
                m_Transform.m_Transform = m_Transform;
                m_Transform.GameObject = this;
                m_Transform.PropertyChanged += OnComponentChanged;
                m_Transform.Awake();
                m_Components.Add(m_Transform);

                m_Enabled = true;
                m_Initialized = false;
                IsStatic = false;
                IsPrefab = false;

                Id = Guid.NewGuid().ToString();
                Name = string.IsNullOrEmpty(name) ? Id : name;

                Scene.current?.Add(this);
            }
        }

        #endregion

        /// <summary>
        /// Initialize and load specific content.
        /// </summary>
        /// <param name="content">The content manager.</param>
        public virtual void Initialize()
        {
            if (!m_Initialized)
            {
                m_Initialized = true;

                // Sort component now then initialize it.
                m_Components.Sort();

                for (int i = 0; i < m_Components.Count; i++)
                {
                    m_Components[i].m_Started = true;
                    m_Components[i].Start();
                }
            }
        }

        /// <summary>
        /// Update components.
        /// </summary>
        public virtual void Update()
        {
            for (int i = 0; i < m_Components.Count; i++)
            {
                if (m_Components[i].Enabled)
                    m_Components[i].Update();
            }
        }

        #region SceneObject collection

        /// <summary>
        /// Add a scene object as children of this scene object. It is not possible to add the same object twice
        /// </summary>
        /// <param name="newGameObject">The scene object to add.</param>
        /// <returns>Return true if added, otherwise return false.</returns>
        public virtual bool Add(GameObject newGameObject)
        {
            if (!m_Transform.Transforms.Contains(newGameObject.m_Transform) && newGameObject != this)
            {
                // Add the scene object to the scene if not yet added.
                if (this != m_Scene)
                {
                    if (m_Scene != null)
                        m_Scene.Add(newGameObject);
                    else
                        throw new Exception("You need to attach first the main scene object to scene.");
                }
                // TODO: Rework this part
                // Remove its parent's transform
                if (newGameObject.Transform.Parent != null)
                    newGameObject.Transform.Parent.Transforms.Remove(newGameObject.Transform);

                // Add to current transform
                newGameObject.m_Transform.Parent = m_Transform;
                newGameObject.m_Transform.Root = m_Transform.Root;
                m_Transform.Transforms.Add(newGameObject.m_Transform);
                newGameObject.Enabled = m_Enabled;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove a scene object. Its transform parent will be the root transform.
        /// </summary>
        /// <param name="sceneObject">The scene object to remove.</param>
        /// <returns>Return true if succed, otherwise return false.</returns>
        public virtual bool Remove(GameObject sceneObject)
        {
            if (sceneObject.m_Transform.Parent == m_Transform)
            {
                m_Transform.Transforms.Remove(sceneObject.m_Transform);
                sceneObject.m_Transform.Parent = m_Transform.Root;
                return true;
            }

            return false;
        }

        #endregion

        #region Component collection

        public Component AddComponent(Component component)
        {
            if (component == null)
                return null;

            var serializedTransform = component as Transform;

            // Called during the deserialization.
            if (serializedTransform != null)
            {
                // The constructor hasn't be called
                InternalConstructor(Name);
                m_Transform.LocalPosition = serializedTransform.LocalPosition;
                m_Transform.LocalRotation = serializedTransform.LocalRotation;
                m_Transform.LocalScale = serializedTransform.LocalScale;
            }
            else
            {
                component.GameObject = this;
                component.m_Transform = m_Transform;
                component.Awake();
                component.PropertyChanged += OnComponentChanged;
                m_Components.Add(component);
            }

            if (m_Initialized && !component.Initialized)
            {
                component.Start();
                component.m_Started = true;
                // Sort components here only if the SceneObject is already initialized.
                m_Components.Sort();
            }

            NotifyComponentChanged(component, string.Empty, ComponentChangeType.Add);

            return component;
        }

        /// <summary>
        /// Add a component of the specified type. Note that you can't add another Transform component.
        /// </summary>
        /// <typeparam name="T">The component's type.</typeparam>
        /// <returns></returns>
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
            for (int i = 0; i < m_Components.Count; i++)
            {
                if (m_Components[i] is T)
                    return m_Components[i] as T;
            }

            return null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            List<T> comps = new List<T>();

            for (int i = 0; i < m_Components.Count; i++)
            {
                if (m_Components[i] is T)
                    comps.Add(m_Components[i] as T);
            }

            return comps.ToArray();
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            var transforms = m_Transform.Transforms;
            var size = transforms.Count;
            var cpns = (Component[])null;

            foreach (var cpn in m_Components)
            {
                if (cpn is T)
                    return (T)cpn;
            }

            for (var i = 0; i < size; i++)
            {
                cpns = transforms[i].GameObject.GetComponentsInChildren<T>();
                foreach (var cpn in cpns)
                {
                    if (cpn is T)
                        return (T)cpn;
                }
            }

            return null;
        }

        public T[] GetComponentsInChildren<T>() where T : Component
        {
            var list = new List<T>();
            list.AddRange(GetComponents<T>());

            foreach (var tr in m_Transform.Transforms)
                list.AddRange(tr.GameObject.GetComponentsInChildren<T>());

            return list.ToArray();
        }

        public T GetComponentInParent<T>() where T : Component
        {
            return m_Transform.Parent.GetComponent<T>();
        }

        public T[] GetComponentsInParent<T>() where T : Component
        {
            return m_Transform.Parent.GetComponents<T>();
        }

        /// <summary>
        /// Remove the component and update the scene.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        /// <returns>Return true if the component has been removed, otherwise return false.</returns>
        public bool RemoveComponent(Component component)
        {
            if (component == m_Transform)
                return false;

            var result = m_Components.Remove(component);
            if (result)
            {
                component.PropertyChanged -= OnComponentChanged;
                NotifyComponentChanged(component, string.Empty, ComponentChangeType.Remove);
            }

            return result;
        }

        protected virtual void OnComponentChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyComponentChanged((Component)sender, e.Name, ComponentChangeType.Update);
        }

        #endregion

        public object Clone()
        {
            var clone = new GameObject(Name.Replace(" (Clone)", "") + " (Clone)");
            Component clonedComponent = null;
            Transform clonedTransform = null;

            foreach (Component component in m_Components)
            {
                clonedComponent = clone.AddComponent((Component)component.Clone());
                clonedComponent.m_GameObject = clone;

                clonedTransform = clonedComponent as Transform;

                if (clonedTransform != null)
                    clonedComponent.m_Transform = clonedTransform;
            }

            clone.Id = "GameObject_" + Guid.NewGuid();
            clone.Tag = Tag;

            return clone;
        }

        public void Dispose()
        {
            foreach (Component component in m_Components)
                component.Dispose();
        }

        public virtual void PostDeserialize()
        {
            var size = m_Components.Count;
            var i = 0;

            if (size > 0)
            {
                m_Transform = GetComponent<Transform>();

                for (i = 0; i < size; i++)
                {
                    m_Components[i].m_GameObject = this;
                    m_Components[i].m_Transform = m_Transform;
                }

                for (i = 0; i < size; i++)
                {
                    m_Components[i].PostDeserialize();
                    m_Components[i].Awake();

                    if (m_Initialized)
                        m_Components[i].Start();
                }

                // Refresh children
                Enabled = m_Enabled;
            }
        }

        #region Static Methods

        public static GameObject Instanciate(GameObject sceneObject)
        {
            return Instanciate(sceneObject, sceneObject.m_Transform.LocalPosition, sceneObject.m_Transform.LocalRotation);
        }

        public static GameObject Instanciate(GameObject sceneObject, Vector3 position, Vector3 rotation)
        {
            GameObject clone = (GameObject)sceneObject.Clone();
            clone.m_Transform.LocalPosition = position;
            clone.m_Transform.LocalRotation = rotation;

            Scene.current.Add(clone);

            return clone;
        }

        public static void Destroy(GameObject sceneObject)
        {
            if (sceneObject != null)
                Scene.current.Remove(sceneObject);
        }

        public static void Destroy(Component component)
        {
            if (component is Transform)
                return;

            component.GameObject.RemoveComponent(component);
        }

        public static GameObject Find(string name)
        {
            var gameObjects = Scene.current.gameObjects;
            foreach (var gameObject in gameObjects)
                if (gameObject.Name == name)
                    return gameObject;

            return null;
        }

        #endregion
    }
}
