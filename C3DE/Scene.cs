using C3DE.Components;
using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace C3DE
{
    public struct RaycastInfo
    {
        public Ray Ray;
        public Collider Collider;
        public float Distance;
    }

    /// <summary>
    /// The scene is responsible to store scene objects.
    /// </summary>
    public class Scene : SceneObject
    {
        public readonly Material DefaultMaterial;

        private ContentManager _content;
        private SmartList<SceneObject> members;
        private List<RenderableComponent> _renderList;
        private List<Material> _materials;
        private List<Collider> _colliders;
        private List<Camera> _cameras;
        private int _mainCameraIndex;
        private List<Light> _lights;
        private Vector4 _ambientColor;
        private List<Behaviour> _scripts;
        private List<Component> _componentsToDestroy;
        private bool _needRemoveCheck;

        public RenderSettings RenderSettings { get; private set; }

        public Camera MainCamera
        {
            get { return _mainCameraIndex > -1 ? _cameras[_mainCameraIndex] : null; }
            set { _mainCameraIndex = Add(value); }
        }

        /// <summary>
        /// Gets the collection of renderable scene objects.
        /// </summary>
        public List<RenderableComponent> RenderList
        {
            get { return _renderList; }
        }

        public List<Material> Materials
        {
            get { return _materials; }
        }

        public List<Collider> Colliders
        {
            get { return _colliders; }
        }

        public List<Light> Lights
        {
            get { return _lights; }
        }

        public List<Camera> Cameras
        {
            get { return _cameras; }
        }

        public List<Behaviour> Behaviours
        {
            get { return _scripts; }
        }

        /// <summary>
        /// The root scene object which contains all scene objects.
        /// </summary>
        public Scene(ContentManager content)
            : base()
        {
            transform.Root = transform;
            isStatic = false;
            members = new SmartList<SceneObject>();
            scene = this;
            _content = content;
            _renderList = new List<RenderableComponent>(10);
            _materials = new List<Material>(5);
            _colliders = new List<Collider>(5);
            _cameras = new List<Camera>(1);
            _mainCameraIndex = -1;
            _scripts = new List<Behaviour>(5);
            _lights = new List<Light>(2);
            _componentsToDestroy = new List<Component>();
            _needRemoveCheck = false;
            _ambientColor = Color.White.ToVector4();
            DefaultMaterial = new SimpleMaterial(this);
            RenderSettings = new RenderSettings();
        }

        #region Lifecycle

        /// <summary>
        /// Load content of all components.
        /// </summary>
        /// <param name="content"></param>
        public override void LoadContent(ContentManager content)
        {
            for (int i = 0, l = _materials.Count; i < l; i++)
                _materials[i].LoadContent(_content);

            for (int i = 0; i < members.Size; i++)
                members[i].LoadContent(_content);

            members.CheckRequired = true;
            initialized = true;
        }

        /// <summary>
        /// Update all scene object.
        /// </summary>
        public override void Update()
        {
            base.Update();

            // First - Check if we need to remove some components.
            if (_needRemoveCheck)
            {
                for (int i = 0, l = _componentsToDestroy.Count; i < l; i++)
                {
                    if (_componentsToDestroy[i] != null)
                    {
                        CheckComponent(_componentsToDestroy[i], ComponentChangeType.Remove);
                        _componentsToDestroy[i] = null;
                    }
                }

                _needRemoveCheck = false;
            }

            // Second - Check if we need to remove some SceneObjectlists.
            members.Check();

            // Third - Safe update
            for (int i = 0; i < members.Size; i++)
            {
                if (members[i].Enabled)
                    members[i].Update();
            }
        }

        #endregion

        #region SceneObjects/Components management

        /// <summary>
        /// Check all components of a scene object to update all list of the scene.
        /// </summary>
        /// <param name="sceneObject">The scene object.</param>
        /// <param name="type">Type of change.</param>
        private void CheckComponents(SceneObject sceneObject, ComponentChangeType type)
        {
            for (int i = 0; i < sceneObject.Components.Count; i++)
                CheckComponent(sceneObject.Components[i], type);
        }

        public void CheckComponent(Component component, ComponentChangeType type)
        {
            if (component is RenderableComponent)
            {
                var renderable = component as RenderableComponent;

                if (type == ComponentChangeType.Add && !_renderList.Contains(renderable))
                    _renderList.Add(renderable);

                else if (type == ComponentChangeType.Remove)
                    _renderList.Remove(renderable);
            }

            else if (component is Behaviour)
            {
                var script = component as Behaviour;

                if (type == ComponentChangeType.Add)
                    Add(script);
                else if (type == ComponentChangeType.Remove)
                    Remove(script);
            }

            else if (component is Collider)
            {
                var collider = component as Collider;

                if (type == ComponentChangeType.Add)
                    Add(collider);
                else if (type == ComponentChangeType.Remove)
                    Remove(collider);
            }

            else if (component is Camera)
            {
                var camera = component as Camera;

                if (type == ComponentChangeType.Add && !_cameras.Contains(camera))
                    Add(camera);
                else if (type == ComponentChangeType.Remove)
                    Remove(camera);
            }

            else if (component is Light)
            {
                var light = component as Light;

                if (type == ComponentChangeType.Add)
                    Add(light);
                else if (type == ComponentChangeType.Remove)
                    Remove(light);
            }
        }

        /// <summary>
        /// Called when a component is added to a registered scene object.
        /// It's actually used to update the render list.
        /// </summary>
        /// <param name="sender">The scene object which as added or removed a component.</param>
        /// <param name="e">An object which contains the component and a flag to know if it's added or removed.</param>
        private void sceneObject_ComponentsChanged(object sender, ComponentChangedEventArgs e)
        {
            CheckComponent(e.Component, e.ChangeType);
        }

        /// <summary>
        /// Add a scene object to the scene.
        /// </summary>
        /// <param name="sceneObject">The scene object to add.</param>
        /// <returns>Return true if the scene object is added, otherwise return false.</returns>
        public override bool Add(SceneObject sceneObject)
        {
            bool canAdd = base.Add(sceneObject);

            if (canAdd)
            {
                members.Add(sceneObject);
                sceneObject.Scene = this;
                sceneObject.Transform.Root = transform;

                if (initialized)
                    sceneObject.LoadContent(_content);

                CheckComponents(sceneObject, ComponentChangeType.Add);

                sceneObject.ComponentChanged += sceneObject_ComponentsChanged;
            }

            return canAdd;
        }

        #endregion

        #region Add/Remove materials

        /// <summary>
        /// Add a new material.
        /// </summary>
        /// <param name="material"></param>
        public void Add(Material material)
        {
            if (!_materials.Contains(material))
            {
                if (initialized)
                    material.LoadContent(_content);

                _materials.Add(material);
            }
        }

        /// <summary>
        /// Remove a material.
        /// </summary>
        /// <param name="material"></param>
        public void Remove(Material material)
        {
            if (_materials.Contains(material))
            {
                _materials.Remove(material);
                material.Dispose();
            }
        }

        #endregion

        #region Add/Remove components

        protected int Add(Camera camera)
        {
            var index = _cameras.IndexOf(camera);

            if (index == -1)
            {
                _cameras.Add(camera);
                index = _cameras.Count - 1;
            }

            if (_mainCameraIndex == -1)
                _mainCameraIndex = index;

            return index;
        }

        protected void Add(Light light)
        {
            if (!_lights.Contains(light))
                _lights.Add(light);
        }

        protected void Add(Collider collider)
        {
            if (!_colliders.Contains(collider))
                _colliders.Add(collider);
        }

        protected void Add(Behaviour script)
        {
            if (!_scripts.Contains(script))
            {
                _scripts.Add(script);
                script.Awake();
                script.Start();
            }
        }

        protected void Remove(Behaviour script)
        {
            if (_scripts.Contains(script))
                _scripts.Remove(script);
        }

        protected void Remove(Light light)
        {
            if (_lights.Contains(light))
                _lights.Remove(light);
        }

        protected void Remove(Collider collider)
        {
            if (_colliders.Contains(collider))
                _colliders.Remove(collider);
        }

        protected void Remove(Camera camera)
        {
            if (_cameras.Contains(camera))
                _cameras.Remove(camera);
        }

        #endregion

        #region Destroy SceneObjects/Components

        private int GetFirstToRemoveComponentNullIndex()
        {
            for (int i = 0, l = _componentsToDestroy.Count; i < l; i++)
            {
                if (_componentsToDestroy[i] == null)
                    return i;
            }

            return -1;
        }

        public void Destroy(SceneObject sceneObject)
        {
            for (int i = 0, l = sceneObject.Components.Count; i < l; i++)
                Destroy(sceneObject.Components[i]);

            members.Remove(sceneObject);
        }

        public void Destroy(Component component)
        {
            var index = GetFirstToRemoveComponentNullIndex();

            if (index > -1)
                _componentsToDestroy[index] = component;
            else
                _componentsToDestroy.Add(component);

            _needRemoveCheck = true;
        }

        #endregion

        #region SceneObject search

        public SceneObject Find(string name)
        {
            for (int i = 0; i < members.Size; i++)
            {
                if (members[i].Name == name)
                    return members[i];
            }

            return null;
        }

        public SceneObject FindObjectOfType<T>() where T : Component, new()
        {
            Component component = null;

            for (int i = 0; i < members.Size; i++)
            {
                component = members[i].GetComponent<T>();
                if (component != null)
                    return members[i];
            }

            return null;
        }

        #endregion

        #region Raycast

        /// <summary>
        /// Cast a ray on collidable objects.
        /// </summary>
        /// <param name="origin">Ray's position.</param>
        /// <param name="direction">Ray's direction.</param>
        /// <param name="distance">Maximum distance.</param>
        /// <returns></returns>
        public bool Raycast(Vector3 origin, Vector3 direction, float distance)
        {
            Ray ray = new Ray(origin, direction);
            float? val;

            for (int i = 0, l = _colliders.Count; i < l; i++)
            {
                val = _colliders[i].IntersectedBy(ref ray);

                if (val.HasValue && val.Value <= distance)
                    return true;
            }

            return false;
        }

        public bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastInfo info)
        {
            info = new RaycastInfo();

            Ray ray = new Ray(origin, direction);
            float? val;
            int i = 0;
            int size = _colliders.Count;
            bool collide = false;

            // A quadtree and even an octree could be very cool in the future :)
            while (i < size && collide == false)
            {
                val = _colliders[i].IntersectedBy(ref ray);

                if (val.HasValue && val.Value <= distance)
                {
                    info.Collider = _colliders[i];
                    info.Distance = val.Value;
                    info.Ray = ray;
                    collide = true;
                }
            }

            return collide;
        }

        #endregion
    }
}
