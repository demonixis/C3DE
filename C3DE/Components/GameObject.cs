using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// A scene object is the base object on the scene.
    /// </summary>
    public class GameObject : ICloneable, IDisposable
    {
        #region Private/protected declarations

        protected Transform _transform;
        protected Scene _scene;
        protected bool _enabled;

        protected List<Component> _components;
        protected bool _initialized;

        #endregion

        #region Fields

        public string Id { get; set; }

        public string Name { get; set; }

        public string Tag { get; set; }

        public bool IsStatic { get; set; }

        public bool IsPrefab { get; set; }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;

                    NotifyPropertyChanged("Enabled");

                    if (_transform != null)
                    {
                        for (int i = 0, l = _transform.Transforms.Count; i < l; i++)
                            _transform.Transforms[i].GameObject.Enabled = value;
                    }
                }
            }
        }

        public Transform transform => _transform;
        public bool activeSelf => Enabled;

        public Transform Transform
        {
            get { return _transform; }
            internal protected set { _transform = value; }
        }

        public Scene Scene
        {
            get { return _scene; }
            internal set { _scene = value; }
        }

        public List<Component> Components
        {
            get { return _components; }
        }

        public bool Initialized
        {
            get { return _initialized; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when a component is added or removed.
        /// </summary>
        public event Action<Component, string, ComponentChangeType> ComponentChanged = null;

        /// <summary>
        /// Called when a registered property has changed.
        /// </summary>
        public event Action<GameObject, string> PropertyChanged = null;

        private void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, property);
        }

        private void NotifyComponentChanged(Component component, string propertyName, ComponentChangeType type)
        {
            ComponentChanged?.Invoke(component, propertyName, type);
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
            if (_transform == null)
            {
                _components = new List<Component>(5);

                _transform = new Transform();
                _transform._transform = _transform;
                _transform.GameObject = this;
                _transform.PropertyChanged += OnComponentChanged;
                _transform.Awake();
                _components.Add(_transform);

                _enabled = true;
                _initialized = false;
                IsStatic = false;
                IsPrefab = false;

                Id = Guid.NewGuid().ToString();
                Name = string.IsNullOrEmpty(name) ? Id : name;

                Scene.current?.Add(this);
            }
        }

        #endregion

        public void SetActive(bool active)
        {
            Enabled = active;
        }

        /// <summary>
        /// Initialize and load specific content.
        /// </summary>
        /// <param name="content">The content manager.</param>
        public virtual void Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;

                // Sort component now then initialize it.
                _components.Sort();

                for (int i = 0; i < _components.Count; i++)
                {
                    _components[i]._started = true;
                    _components[i].Start();
                }
            }
        }

        /// <summary>
        /// Update components.
        /// </summary>
        public virtual void Update()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Enabled)
                    _components[i].Update();
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
            if (!_transform.Transforms.Contains(newGameObject._transform) && newGameObject != this)
            {
                // Add the scene object to the scene if not yet added.
                if (this != _scene)
                {
                    if (_scene != null)
                        _scene.Add(newGameObject);
                    else
                        throw new Exception("You need to attach first the main scene object to scene.");
                }
                // TODO: Rework this part
                // Remove its parent's transform
                if (newGameObject.Transform.Parent != null)
                    newGameObject.Transform.Parent.Transforms.Remove(newGameObject.Transform);

                // Add to current transform
                newGameObject._transform.Parent = _transform;
                newGameObject._transform.Root = _transform.Root;
                _transform.Transforms.Add(newGameObject._transform);
                newGameObject.Enabled = _enabled;

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
            if (sceneObject._transform.Parent == _transform)
            {
                _transform.Transforms.Remove(sceneObject._transform);
                sceneObject._transform.Parent = _transform.Root;
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
                _transform.LocalPosition = serializedTransform.LocalPosition;
                _transform.LocalRotation = serializedTransform.LocalRotation;
                _transform.LocalScale = serializedTransform.LocalScale;
            }
            else
            {
                component.GameObject = this;
                component._transform = _transform;
                component.Awake();
                component.PropertyChanged += OnComponentChanged;
                _components.Add(component);
            }

            if (_initialized && !component.Initialized)
            {
                component.Start();
                component._started = true;
                // Sort components here only if the SceneObject is already initialized.
                _components.Sort();
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
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T)
                    return _components[i] as T;
            }

            return null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            List<T> comps = new List<T>();

            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T)
                    comps.Add(_components[i] as T);
            }

            return comps.ToArray();
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            var transforms = _transform.Transforms;
            var size = transforms.Count;
            var cpns = (Component[])null;

            foreach (var cpn in _components)
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

            foreach (var tr in _transform.Transforms)
                list.AddRange(tr.GameObject.GetComponentsInChildren<T>());

            return list.ToArray();
        }

        public T GetComponentInParent<T>() where T : Component
        {
            return _transform.Parent.GetComponent<T>();
        }

        public T[] GetComponentsInParent<T>() where T : Component
        {
            return _transform.Parent.GetComponents<T>();
        }

        /// <summary>
        /// Remove the component and update the scene.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        /// <returns>Return true if the component has been removed, otherwise return false.</returns>
        public bool RemoveComponent(Component component)
        {
            if (component == _transform)
                return false;

            var result = _components.Remove(component);
            if (result)
            {
                component.PropertyChanged -= OnComponentChanged;
                NotifyComponentChanged(component, string.Empty, ComponentChangeType.Remove);
            }

            return result;
        }

        protected virtual void OnComponentChanged(Component component, string name)
        {
            NotifyComponentChanged(component, name, ComponentChangeType.Update);
        }

        #endregion

        public object Clone()
        {
            var clone = new GameObject(Name.Replace(" (Clone)", "") + " (Clone)");
            Component clonedComponent = null;
            Transform clonedTransform = null;

            foreach (Component component in _components)
            {
                clonedComponent = clone.AddComponent((Component)component.Clone());
                clonedComponent._gameObject = clone;

                clonedTransform = clonedComponent as Transform;

                if (clonedTransform != null)
                    clonedComponent._transform = clonedTransform;
            }

            clone.Id = "GameObject_" + Guid.NewGuid();
            clone.Tag = Tag;

            return clone;
        }

        public void Dispose()
        {
            foreach (Component component in _components)
                component.Dispose();
        }

        public virtual void PostDeserialize()
        {
            var size = _components.Count;
            var i = 0;

            if (size > 0)
            {
                _transform = GetComponent<Transform>();

                for (i = 0; i < size; i++)
                {
                    _components[i]._gameObject = this;
                    _components[i]._transform = _transform;
                }

                for (i = 0; i < size; i++)
                {
                    _components[i].PostDeserialize();
                    _components[i].Awake();

                    if (_initialized)
                        _components[i].Start();
                }

                // Refresh children
                Enabled = _enabled;
            }
        }

        #region Static Methods

        public static GameObject CreatePrimitive(PrimitiveTypes type, Material material = null)
        {
            var gameObject = new GameObject();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();

            switch (type)
            {
                case PrimitiveTypes.Cube: meshRenderer.Mesh = new CubeMesh(); break;
                case PrimitiveTypes.Cylinder: meshRenderer.Mesh = new CylinderMesh(); break;
                case PrimitiveTypes.Plane: meshRenderer.Mesh = new PlaneMesh(); break;
                case PrimitiveTypes.Pyramid: meshRenderer.Mesh = new PyramidMesh(); break;
                case PrimitiveTypes.Quad: meshRenderer.Mesh = new QuadMesh(); break;
                case PrimitiveTypes.Sphere: meshRenderer.Mesh = new SphereMesh(); break;
                case PrimitiveTypes.Torus: meshRenderer.Mesh = new TorusMesh(); break;
            }

            meshRenderer.Mesh.Build();
            meshRenderer.Material = material ?? Scene.current?._defaultMaterial;

            gameObject.AddComponent<SphereCollider>();
            gameObject.Transform.Translate(0, meshRenderer.BoundingSphere.Radius, 0);

            return gameObject;
        }

        public static GameObject Instantiate(GameObject sceneObject)
        {
            return Instanciate(sceneObject, sceneObject._transform.LocalPosition, sceneObject._transform.LocalRotation);
        }

        public static GameObject Instanciate(GameObject sceneObject, Vector3 position, Vector3 rotation)
        {
            GameObject clone = (GameObject)sceneObject.Clone();
            clone._transform.LocalPosition = position;
            clone._transform.LocalRotation = rotation;

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
            if (component == null || component is Transform)
                return;

            component.GameObject.RemoveComponent(component);
        }

        public static GameObject Find(string name)
        {
            var gameObjects = Scene.current._gameObjects;
            foreach (var gameObject in gameObjects)
                if (gameObject.Name == name)
                    return gameObject;

            return null;
        }

        #endregion
    }
}
