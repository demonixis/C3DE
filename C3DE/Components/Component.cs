using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace C3DE.Components
{
    /// <summary>
    /// A component is a part of a scene object.
    /// </summary>
    [DataContract]
    public abstract class Component : IComparable, ICloneable, IDisposable
    {
        internal protected bool m_Started;
        protected bool m_Enabled;
        protected int m_Order = 1;
        internal protected GameObject m_GameObject;
        internal protected Transform m_Transform;

        #region Fields

        [DataMember]
        public bool Enabled
        {
            get { return m_Enabled; }
            set
            {
                if (value != m_Enabled)
                {
                    m_Enabled = value;

                    if (m_Enabled)
                        OnEnabled();
                    else
                        OnDisabled();

                    NotifyPropertyChanged("Enabled");
                }
            }
        }

        public bool Initialized
        {
            get { return m_Started; }
        }

        [DataMember]
        public string Id { get; set; }

        public GameObject GameObject
        {
            get { return m_GameObject; }
            internal set { m_GameObject = value; }
        }

        public Transform Transform
        {
            get { return m_Transform; }
            internal set { m_Transform = value; }
        }

        [DataMember]
        public int Order
        {
            get { return m_Order; }
            protected set
            {
                if (value != m_Order)
                {
                    m_Order = value;
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
            m_Started = false;
            m_Enabled = true;
            Id = "CPN-" + Guid.NewGuid();
        }

        public virtual void OnEnabled()
        {
        }

        public virtual void OnDisabled()
        {
        }

        public virtual void Awake()
        {
            if (m_Transform == null)
                m_Transform = GetComponent<Transform>();
        }

        /// <summary>
        /// Sets the initialized flag to true.
        /// </summary>
        /// <param name="content"></param>
        public virtual void Start()
        {
            m_Started = true;
        }

        /// <summary>
        /// Update the logic.
        /// </summary>
        public virtual void Update()
        {
        }

        public virtual int CompareTo(object obj)
        {
            var component = obj as Component;

            if (component == null)
                return 1;

            if (m_Order == component.m_Order)
                return 0;
            else if (m_Order > component.m_Order)
                return 1;
            else
                return -1;
        }

#region Add / Get Component

        public T AddComponent<T>() where T : Component, new()
        {
            return m_GameObject.AddComponent<T>();
        }

        public T GetComponent<T>() where T : Component
        {
            return m_GameObject.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : Component
        {
            return m_GameObject.GetComponents<T>();
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            return m_GameObject.GetComponentInChildren<T>();
        }

        public T[] GetComponentsInChildren<T>() where T : Component
        {
            return m_GameObject.GetComponentsInChildren<T>();
        }

        public T GetComponentInParent<T>() where T : Component
        {
            return m_GameObject.GetComponentInParent<T>();
        }

        public T[] GetComponentsInParent<T>() where T : Component
        {
            return m_GameObject.GetComponentsInParent<T>();
        }

        #endregion

        public virtual void Reset()
        {
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public virtual void Dispose()
        {
        }

        public virtual void PostDeserialize()
        {
        }
    }
}