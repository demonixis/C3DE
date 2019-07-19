using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Jitter.Collision;
using Jitter;

namespace C3DE
{
    public class SerializedScene
    {
        public RenderSettings RenderSettings { get; set; }
        public GameObject[] GameObjects { get; set; }
        public Material[] Materials { get; set; }
    }

    public struct RaycastInfo
    {
        public Vector3 Position;
        public Ray Ray;
        public Collider Collider;
        public float Distance;
    }

    /// <summary>
    /// The scene is responsible to store scene objects, components.
    /// </summary>
    public class Scene : GameObject
    {
        public static Scene current { get; internal set; }

        private List<Component> _componentsToDestroy;
        private bool _needRemoveCheck;

        internal protected Material _defaultMaterial;
        internal protected List<Material> _materials;
        internal protected List<GameObject> _gameObjects;

        internal protected Dictionary<Renderer, List<Transform>> _instances;
        internal protected List<Renderer> _renderList;
        internal protected List<PostProcessPass> _postProcessPasses;
        internal protected List<PlanarReflection> _planarReflections;
        internal protected List<ReflectionProbe> _reflectionProbes;
        internal protected List<Collider> _colliders;
        internal protected List<Camera> _cameras;
        internal protected List<Light> _lights;
        internal protected List<Behaviour> _scripts;

        internal protected CollisionSystem _physicsCollisionSystem;
        internal protected World _physicsWorld;

        public RenderSettings RenderSettings { get; private set; }

        /// <summary>
        /// The root scene object which contains all scene objects.
        /// </summary>
        public Scene()
            : base()
        {
            Name = "Scene-" + Guid.NewGuid();
            RenderSettings = new RenderSettings();

            _transform.Root = _transform;
            _gameObjects = new List<GameObject>();
            _needRemoveCheck = false;
            _scene = this;
            _renderList = new List<Renderer>();
            _materials = new List<Material>();
            _defaultMaterial = new UnlitMaterial();

            // Components
            _colliders = new List<Collider>();
            _cameras = new List<Camera>();
            _scripts = new List<Behaviour>();
            _lights = new List<Light>();
            _postProcessPasses = new List<PostProcessPass>();
            _componentsToDestroy = new List<Component>();
            _planarReflections = new List<PlanarReflection>();
            _reflectionProbes = new List<ReflectionProbe>();

            // Physics
            _physicsCollisionSystem = new CollisionSystemSAP();
            _physicsWorld = new World(_physicsCollisionSystem);
        }

        public Scene(string name)
            : this()
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;
        }

        #region Lifecycle

        /// <summary>
        /// Initialize the scene. This method is called whenever the scene is used by
        /// the SceneManager.
        /// </summary>
        /// <param name="content"></param>
        public override void Initialize()
        {
            _initialized = true;

            RenderSettings.Skybox.LoadContent(Application.Content);

            for (var i = 0; i < _materials.Count; i++)
                _materials[i].LoadContent(Application.Content);

            for (int i = 0; i < _gameObjects.Count; i++)
                _gameObjects[i].Initialize();
        }

        /// <summary>
        /// Update all scene object.
        /// </summary>
        public override void Update()
        {
            base.Update();

            _physicsWorld.Step(Time.DeltaTime, true);

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

            // Third - Safe update
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i].Enabled)
                    _gameObjects[i].Update();
            }
        }

        /// <summary>
        /// Unload the scene.
        /// </summary>
        public virtual void Unload()
        {
            foreach (Behaviour script in _scripts)
                script.OnDestroy();

            foreach (GameObject gameObject in _gameObjects)
                gameObject.Dispose();

            foreach (Material material in _materials)
                material.Dispose();

            foreach (PostProcessPass pass in _postProcessPasses)
                pass.Dispose();

            Clear();
        }

        /// <summary>
        /// Clean the scene.
        /// </summary>
        protected void Clear()
        {
            _renderList.Clear();
            _materials.Clear();
            _colliders.Clear();
            _cameras.Clear();
            _lights.Clear();
            _scripts.Clear();
            _gameObjects.Clear();
            _postProcessPasses.Clear();
            _planarReflections.Clear();
            _componentsToDestroy.Clear();
            _reflectionProbes.Clear();
            _needRemoveCheck = false;
        }

        #endregion

        #region GameObjects/Components management

        public override bool Add(GameObject gameObject)
        {
            return Add(gameObject, false);
        }

        public bool Add(GameObject gameObject, bool noCheck)
        {
            bool canAdd = base.Add(gameObject);

            if (canAdd)
            {
                _gameObjects.Add(gameObject);
                gameObject.Scene = this;
                gameObject.Transform.Root = _transform;

                if (gameObject.Enabled)
                {
                    CheckComponents(gameObject, ComponentChangeType.Add);
                    gameObject.PropertyChanged += OnGameObjectPropertyChanged;
                    gameObject.ComponentChanged += OnGameObjectComponentChanged;
                }

                if (_initialized && !gameObject.Initialized)
                    gameObject.Initialize();
            }

            return canAdd;
        }

        /// <summary>
        /// Check all components of a scene object to update all list of the scene.
        /// </summary>
        /// <param name="gameObject">The scene object.</param>
        /// <param name="type">Type of change.</param>
        protected void CheckComponents(GameObject gameObject, ComponentChangeType type)
        {
            for (int i = 0; i < gameObject.Components.Count; i++)
                CheckComponent(gameObject.Components[i], type);
        }

        /// <summary>
        /// Check a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="type"></param>
        protected void CheckComponent(Component component, ComponentChangeType type)
        {
            if (type == ComponentChangeType.Update)
                return;

            var added = type == ComponentChangeType.Add;

            if (component is Renderer)
                SetComponent((Renderer)component, _renderList, added);
            else if (component is Collider)
                SetComponent((Collider)component, _colliders, added);
            else if (component is Light)
                SetComponent((Light)component, _lights, added);
            else if (component is Behaviour)
                SetComponent((Behaviour)component, _scripts, added);
            else if (component is Camera)
                SetComponent((Camera)component, _cameras, added);
            else if (component is PlanarReflection)
                SetComponent((PlanarReflection)component, _planarReflections, added);
            else if (component is ReflectionProbe)
                SetComponent((ReflectionProbe)component, _reflectionProbes, added);
        }

        private void OnGameObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Name == "Enabled")
            {
                var gameObject = (GameObject)sender;
                if (gameObject.Enabled)
                {
                    CheckComponents(gameObject, ComponentChangeType.Add);
                    gameObject.ComponentChanged += OnGameObjectComponentChanged;
                }
                else
                {
                    CheckComponents(gameObject, ComponentChangeType.Remove);
                    gameObject.ComponentChanged -= OnGameObjectComponentChanged;
                }
            }
        }

        /// <summary>
        /// Called when a component is added to a registered scene object.
        /// It's actually used to update the render list.
        /// </summary>
        /// <param name="sender">The scene object which as added or removed a component.</param>
        /// <param name="e">An object which contains the component and a flag to know if it's added or removed.</param>
        private void OnGameObjectComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.ChangeType == ComponentChangeType.Update)
            {
                if (e.PropertyName == "Enabled")
                    CheckComponent(e.Component, e.Component.Enabled ? ComponentChangeType.Add : ComponentChangeType.Remove);
            }
            else
                CheckComponent(e.Component, e.ChangeType);
        }

        #endregion

        #region Add / Get / Remove

        /// <summary>
        /// Add a new material.
        /// </summary>
        /// <param name="material"></param>
        internal protected void SetMaterial(Material material, bool added)
        {
            if (added)
            {
                if (!_materials.Contains(material))
                {
                    _materials.Add(material);

                    if (Initialized)
                        material.LoadContent(Application.Content);
                }
            }
            else
            {
                _materials.Remove(material);
                material.Dispose();
            }
        }


        private void SetComponent<T>(T component, List<T> list, bool added) where T : Component
        {
            if (added)
            {
                if (!list.Contains(component))
                {
                    list.Add(component);

                    if (component is Camera)
                    {
                        var camera = component as Camera;
                        if (Camera.Main == null)
                            Camera.Main = camera;
                    }
                }
            }
            else
                list.Remove(component);

            list.Sort();
        }

        #endregion

        #region Destroy GameObjects/Components

        private int GetFirstNullRemovedComponent()
        {
            for (int i = 0, l = _componentsToDestroy.Count; i < l; i++)
            {
                if (_componentsToDestroy[i] == null)
                    return i;
            }

            return -1;
        }

        public override bool Remove(GameObject gameObject)
        {
            return Remove(gameObject, false);
        }

        public bool Remove(GameObject gameObject, bool noCheck = false)
        {
            bool canRemove = base.Remove(gameObject);

            if (canRemove)
                DestroyObject(gameObject, noCheck);

            return canRemove;
        }

        public void DestroyObject(GameObject gameObject, bool noCheck = false)
        {
            for (int i = 0, l = gameObject.Components.Count; i < l; i++)
                this.DestroyComponent(gameObject.Components[i]);

            _gameObjects.Remove(gameObject);
        }

        public void DestroyComponent(Component component)
        {
            var index = GetFirstNullRemovedComponent();

            if (index > -1)
                _componentsToDestroy[index] = component;
            else
                _componentsToDestroy.Add(component);

            _needRemoveCheck = true;
        }

        #endregion

        #region Add/Remove PostProcess

        public void SetPostProcess(PostProcessPass pass, bool added)
        {
            if (added)
            {
                if (!_postProcessPasses.Contains(pass))
                {
                    _postProcessPasses.Add(pass);
                    pass.Initialize(Application.Content);
                }
            }
            else
                _postProcessPasses.Remove(pass);
        }

        #endregion

        #region Search methods

        public static GameObject FindById(string id)
        {
            if (current != null)
            {
                for (int i = 0; i < current._gameObjects.Count; i++)
                    if (current._gameObjects[i].Id == id)
                        return current._gameObjects[i];
            }
            return null;
        }

        public static GameObject[] FindGameObjectsById(string id)
        {
            var gameObjects = new List<GameObject>();

            if (current != null)
            {
                for (int i = 0; i < current._gameObjects.Count; i++)
                    if (current._gameObjects[i].Id == id)
                        gameObjects.Add(current._gameObjects[i]);
            }

            return gameObjects.ToArray();
        }

        public static T FindObjectOfType<T>() where T : Component
        {
            var scripts = new List<T>();

            if (current != null)
            {
                foreach (GameObject so in current._gameObjects)
                {
                    var components = so.GetComponents<T>();
                    if (components.Length > 0)
                        return components[0];
                }
            }

            return default(T);
        }

        public static T[] FindObjectsOfType<T>() where T : Component
        {
            var scripts = new List<T>();

            if (current != null)
            {
                foreach (GameObject so in current._gameObjects)
                {
                    var components = so.GetComponents<T>();
                    if (components.Length > 0)
                        scripts.AddRange(components);
                }
            }

            return scripts.ToArray();
        }

        public Material GetMaterialByName(string name)
        {
            foreach (var mat in _materials)
                if (mat.Name == name)
                    return mat;
            return null;
        }

        #endregion

        #region Collisions detection

        public Collider Collides(Collider collider)
        {
            for (int i = 0, l = _colliders.Count; i < l; i++)
            {
                if (collider.Collides(_colliders[i]))
                    return _colliders[i];
            }

            return null;
        }

        #endregion

        #region Raycast

        private bool Raycast(Ray ray, float distance = 1000.0f)
        {
            float? val;

            for (int i = 0, l = _colliders.Count; i < l; i++)
            {
                val = _colliders[i].IntersectedBy(ref ray);

                if (val.HasValue && val.Value <= distance)
                    return true;
            }

            return false;
        }

        public bool Raycast(Ray ray, float distance, out RaycastInfo info)
        {
            info = new RaycastInfo();
            RaycastInfo[] infos;
            RaycastAll(ray, distance, out infos);

            var size = infos.Length;
            if (size > 0)
            {
                var min = float.MaxValue;
                var index = -1;

                for (int i = 0; i < size; i++)
                {
                    if (infos[i].Distance < min)
                    {
                        min = infos[i].Distance;
                        index = i;
                    }
                }

                if (index > -1)
                    info = infos[index];
            }

            return size > 0;
        }

        public bool Raycast(Vector3 origin, Vector3 direction, float distance = 1000.0f)
        {
            return Raycast(new Ray(origin, direction), distance);
        }

        public bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastInfo info)
        {
            return Raycast(new Ray(origin, direction), distance, out info);
        }

        public bool RaycastAll(Ray ray, float distance, out RaycastInfo[] raycastInfos)
        {
            var infos = new List<RaycastInfo>();

            for (int i = 0, l = _colliders.Count; i < l; i++)
                TestCollision(ref ray, _colliders[i], distance, infos);

            raycastInfos = infos.ToArray();

            return raycastInfos.Length > 0;
        }

        private void TestCollision(ref Ray ray, Collider collider, float distance, List<RaycastInfo> infos)
        {
            if (collider.IsPickable)
            {
                var val = collider.IntersectedBy(ref ray);

                if (val.HasValue && val.Value <= distance)
                {

                    infos.Add(new RaycastInfo()
                    {
                        Position = ray.Position,
                        Collider = collider,
                        Distance = val.Value,
                        Ray = ray
                    });
                }
            }
        }

        public bool RaycastAll(Vector3 origin, Vector3 direction, float distance, out RaycastInfo[] infos)
        {
            return RaycastAll(new Ray(origin, direction), distance, out infos);
        }

        #endregion
    }
}
