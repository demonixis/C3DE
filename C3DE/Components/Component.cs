using C3DE.Serialization;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace C3DE.Components
{
    /// <summary>
    /// A component is a part of a scene object.
    /// </summary>
    public abstract class Component : IComparable, ICloneable, IDisposable, ISerializable
    {
        internal protected bool initialized;
        protected bool enabled;
        protected int order = 1;
        protected SceneObject sceneObject;
        protected Transform transform;

        #region Fields

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (SetActive(value))
                    NotifyPropertyChanged("Enabled");
            }
        }

        public bool Initialized
        {
            get { return initialized; }
        }

        public string Id { get; set; }

        /// <summary>
        /// Gets the scene object of this component.
        /// </summary>
        public SceneObject SceneObject
        {
            get { return sceneObject; }
            internal set { sceneObject = value; }
        }

        public Transform Transform
        {
            get { return transform; }
            internal set { transform = value; }
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

        #endregion

        #region Events

        public event EventHandler<PropertyChangedEventArgs> PropertyChanged = null;

        protected void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        /// <summary>
        /// Create an empty component.
        /// </summary>
        public Component()
        {
            initialized = false;
            enabled = true;
            Id = "CPN-" + Guid.NewGuid();
        }

        public virtual void OnEnabled()
        {
        }

        public virtual void OnDisabled()
        {
        }

        public virtual bool SetActive(bool value)
        {
            if (value != enabled)
            {
                enabled = value;

                if (enabled)
                    OnEnabled();
                else
                    OnDisabled();

				return true;
            }

			return false;
        }

        public virtual void Awake()
        {
            transform = GetComponent<Transform>();
        }

        /// <summary>
        /// Sets the initialized flag to true.
        /// </summary>
        /// <param name="content"></param>
        public virtual void Start()
        {
            initialized = true;
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

            if (component == null)
                return 1;

            if (order == component.order)
                return 0;
            else if (order > component.order)
                return 1;
            else
                return -1;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            return sceneObject.AddComponent<T>();
        }

        public T GetComponent<T>() where T : Component
        {
            return sceneObject.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : Component
        {
            return sceneObject.GetComponents<T>();
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public virtual void Dispose()
        {
        }

        public virtual Dictionary<string, object> Serialize()
        {
            var data = new Dictionary<string, object>();
            data.Add("Enabled", Enabled);
            data.Add("Order", Order);
            data.Add("Type", GetType().FullName);
            return data;
        }

        public virtual void Deserialize(Dictionary<string, object> data)
        {
            enabled =(bool)data["Enabled"];
            order = (int)data["Order"];
        }
    }
}