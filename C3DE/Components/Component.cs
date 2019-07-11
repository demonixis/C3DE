using System;

namespace C3DE.Components
{
    /// <summary>
    /// A component is a part of a scene object.
    /// </summary>
    public abstract class Component : IComparable, ICloneable, IDisposable
    {
        internal protected bool _started;
        protected bool _enabled;
        protected int _order = 1;
        internal protected GameObject _gameObject;
        internal protected Transform _transform;

        #region Fields

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;

                    if (_enabled)
                        OnEnabled();
                    else
                        OnDisabled();

                    NotifyPropertyChanged("Enabled");
                }
            }
        }

        public bool Initialized => _started;

        public string Id { get; set; }

        public GameObject GameObject
        {
            get { return _gameObject; }
            internal set { _gameObject = value; }
        }

        public Transform Transform
        {
            get { return _transform; }
            internal set { _transform = value; }
        }

        public int Order
        {
            get { return _order; }
            protected set
            {
                if (value != _order)
                {
                    _order = value;
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
            _started = false;
            _enabled = true;
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
            if (_transform == null)
                _transform = GetComponent<Transform>();
        }

        /// <summary>
        /// Sets the initialized flag to true.
        /// </summary>
        /// <param name="content"></param>
        public virtual void Start()
        {
            _started = true;
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

            if (_order == component._order)
                return 0;
            else if (_order > component._order)
                return 1;
            else
                return -1;
        }

        #region Add / Get Component

        public T AddComponent<T>() where T : Component, new()
        {
            return _gameObject.AddComponent<T>();
        }

        public T GetComponent<T>() where T : Component
        {
            return _gameObject.GetComponent<T>();
        }

        public T[] GetComponents<T>() where T : Component
        {
            return _gameObject.GetComponents<T>();
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            return _gameObject.GetComponentInChildren<T>();
        }

        public T[] GetComponentsInChildren<T>() where T : Component
        {
            return _gameObject.GetComponentsInChildren<T>();
        }

        public T GetComponentInParent<T>() where T : Component
        {
            return _gameObject.GetComponentInParent<T>();
        }

        public T[] GetComponentsInParent<T>() where T : Component
        {
            return _gameObject.GetComponentsInParent<T>();
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