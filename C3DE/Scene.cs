using C3DE.Components;
using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// The scene is responsible to store scene objects, components.
    /// </summary>
    public class Scene : SceneObject
    {
        public readonly Material DefaultMaterial;

        internal protected SmartList<SceneObject> members;
        internal protected List<RenderableComponent> renderList;
        internal protected List<Material> materials;
        internal protected List<Effect> effects;
        internal protected Dictionary<int, int> materialsEffectIndex;
        internal protected List<Collider> colliders;
        internal protected List<Camera> cameras;
        internal protected List<Light> lights;
        internal protected List<Behaviour> scripts;

        private int _mainCameraIndex;
        private List<Component> _componentsToDestroy;
        private bool _needRemoveCheck;

        public RenderSettings RenderSettings { get; private set; }

        public Camera MainCamera
        {
            get { return _mainCameraIndex > -1 ? cameras[_mainCameraIndex] : null; }
            set { _mainCameraIndex = Add(value); }
        }

        /// <summary>
        /// Gets the collection of renderable scene objects.
        /// </summary>
        public List<RenderableComponent> RenderList
        {
            get { return renderList; }
        }

        public List<Material> Materials
        {
            get { return materials; }
        }

        public List<Collider> Colliders
        {
            get { return colliders; }
        }

        public List<Light> Lights
        {
            get { return lights; }
        }

        public List<Camera> Cameras
        {
            get { return cameras; }
        }

        public List<Behaviour> Behaviours
        {
            get { return scripts; }
        }

        /// <summary>
        /// The root scene object which contains all scene objects.
        /// </summary>
        public Scene(string name)
            : base()
        {
            Name = name;
            transform.Root = transform;
            members = new SmartList<SceneObject>();
            scene = this;
            renderList = new List<RenderableComponent>(10);
            materials = new List<Material>(5);
            effects = new List<Effect>(5);
            materialsEffectIndex = new Dictionary<int, int>(5);
            colliders = new List<Collider>(5);
            cameras = new List<Camera>(1);
            _mainCameraIndex = -1;
            scripts = new List<Behaviour>(5);
            lights = new List<Light>(2);
            _componentsToDestroy = new List<Component>();
            _needRemoveCheck = false;
            DefaultMaterial = new SimpleMaterial(this);
            RenderSettings = new RenderSettings();
        }

        #region Lifecycle

        /// <summary>
        /// Load content of all components.
        /// </summary>
        /// <param name="content"></param>
        public override void Initialize()
        {
            DefaultMaterial.MainTexture = GraphicsHelper.CreateTexture(Color.AntiqueWhite, 1, 1);

            for (int i = 0, l = materials.Count; i < l; i++)
                materials[i].LoadContent(Application.Content);

            UpdateEffectMaterialMatching();

            for (int i = 0; i < members.Size; i++)
                members[i].Initialize();

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

        public void Unload()
        {
       
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

                if (type == ComponentChangeType.Add)
                    Add(renderable);

                else if (type == ComponentChangeType.Remove)
                    Remove(renderable);
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

                if (type == ComponentChangeType.Add && !cameras.Contains(camera))
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
                    sceneObject.Initialize();

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
            if (!materials.Contains(material))
            {
                materials.Add(material);

                if (initialized)
                {
                    material.LoadContent(Application.Content);

                    if (!effects.Contains(material.effect))
                    {
                        effects.Add(material.effect);
                        materialsEffectIndex.Add(effects.IndexOf(material.effect), materials.IndexOf(material));
                    }
                }
            }
        }

        /// <summary>
        /// Remove a material.
        /// </summary>
        /// <param name="material"></param>
        public void Remove(Material material)
        {
            if (materials.Contains(material))
            {
                materials.Remove(material);
                material.Dispose();
            }
        }

        private void UpdateEffectMaterialMatching()
        {
            for (int i = 0, l = materials.Count; i < l; i++)
            {
                if (!effects.Contains(materials[i].effect))
                {
                    effects.Add(materials[i].effect);
                    materialsEffectIndex.Add(effects.IndexOf(materials[i].effect), materials.IndexOf(materials[i]));
                }
            }
        }

        #endregion

        #region Add/Remove components

        protected int Add(Camera camera)
        {
            var index = cameras.IndexOf(camera);

            if (index == -1)
            {
                cameras.Add(camera);
                index = cameras.Count - 1;
            }

            if (_mainCameraIndex == -1)
                _mainCameraIndex = index;

            return index;
        }

        protected void Add(RenderableComponent renderable)
        {
            if (!renderList.Contains(renderable))
                renderList.Add(renderable);
        }

        protected void Add(Light light)
        {
            if (!lights.Contains(light))
                lights.Add(light);
        }

        protected void Add(Collider collider)
        {
            if (!colliders.Contains(collider))
                colliders.Add(collider);
        }

        protected void Add(Behaviour script)
        {
            if (!scripts.Contains(script))
                scripts.Add(script);
        }

        protected void Remove(RenderableComponent renderable)
        {
            if (renderList.Contains(renderable))
                renderList.Remove(renderable);
        }

        protected void Remove(Behaviour script)
        {
            if (scripts.Contains(script))
                scripts.Remove(script);
        }

        protected void Remove(Light light)
        {
            if (lights.Contains(light))
                lights.Remove(light);
        }

        protected void Remove(Collider collider)
        {
            if (colliders.Contains(collider))
                colliders.Remove(collider);
        }

        protected void Remove(Camera camera)
        {
            if (cameras.Contains(camera))
                cameras.Remove(camera);
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

        private bool Raycast(Ray ray, float distance = 1000.0f)
        {
            float? val;

            for (int i = 0, l = colliders.Count; i < l; i++)
            {
                val = colliders[i].IntersectedBy(ref ray);

                if (val.HasValue && val.Value <= distance)
                    return true;
            }

            return false;
        }

        public bool Raycast(Ray ray, float distance, out RaycastInfo info)
        {
            info = new RaycastInfo();

            float? val;
            int i = 0;
            int size = colliders.Count;
            bool collide = false;

            // A quadtree and even an octree could be very cool in the future :)
            while (i < size && collide == false)
            {
                if (colliders[i].IsPickable)
                {
                    val = colliders[i].IntersectedBy(ref ray);

                    if (val.HasValue && val.Value <= distance)
                    {
                        info.Collider = colliders[i];
                        info.Distance = val.Value;
                        info.Ray = ray;
                        collide = true;
                    }
                }

                i++;
            }

            return collide;
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
            List<RaycastInfo> infos = new List<RaycastInfo>();
            RaycastInfo info = new RaycastInfo();
            float? val;

            for (int i = 0, l = colliders.Count; i < l; i++)
            {
                if (colliders[i].IsPickable)
                {
                    val = colliders[i].IntersectedBy(ref ray);

                    if (val.HasValue && val.Value <= distance)
                    {
                        info.Collider = colliders[i];
                        info.Distance = val.Value;
                        info.Ray = ray;
                        infos.Add(info);
                    }
                }
            }

            raycastInfos = infos.ToArray();

            return raycastInfos.Length > 0;
        }

        public bool RaycastAll(Vector3 origin, Vector3 direction, float distance, out RaycastInfo[] infos)
        {
            return RaycastAll(new Ray(origin, direction), distance, out infos);
        }

        #endregion
    }
}
