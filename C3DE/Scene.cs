using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Jitter.Collision;
using Jitter;

namespace C3DE
{
    [DataContract]
    public class SerializedScene
    {
        [DataMember]
        public RenderSettings RenderSettings { get; set; }

        [DataMember]
        public GameObject[] GameObjects { get; set; }

        [DataMember]
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
    [DataContract]
    public class Scene : GameObject
    {
        public static Scene current { get; internal set; }

        private List<Component> _componentsToDestroy;
        private bool _needRemoveCheck;

        internal protected Material defaultMaterial;

        [DataMember]
        internal protected List<GameObject> sceneObjects;

        internal protected List<Renderer> renderList;

        [DataMember]
        internal protected List<Material> materials;
        internal protected List<Effect> effects;
        internal protected Dictionary<int, int> materialsEffectIndex;
        internal protected List<Collider> colliders;
        internal protected List<Camera> cameras;
        internal protected List<Light> lights;
        internal protected List<Behaviour> scripts;

        [DataMember]
        internal protected List<GameObject> prefabs;
        internal protected List<PostProcessPass> postProcessPasses;

        internal protected CollisionSystem m_PhysicsCollisionSystem;
        internal protected World m_PhysicsWorld;

        public RenderSettings RenderSettings { get; private set; }

        public Material DefaultMaterial
        {
            get { return defaultMaterial; }
            set
            {
                if (value == null)
                    throw new Exception("The default material can't be null");

                m_Scene.RemoveMaterial(value);
                defaultMaterial = value;
                defaultMaterial.Name = "Default Material";
            }
        }

        /// <summary>
        /// Gets the collection of renderable scene objects.
        /// </summary>
        public List<Renderer> RenderList
        {
            get { return renderList; }
        }

        /// <summary>
        /// Gets materials.
        /// </summary>
        public List<Material> Materials
        {
            get { return materials; }
        }

        /// <summary>
        /// Gets colliders.
        /// </summary>
        public List<Collider> Colliders
        {
            get { return colliders; }
        }

        /// <summary>
        /// Gets lights.
        /// </summary>
        public List<Light> Lights
        {
            get { return lights; }
        }

        /// <summary>
        /// Gets cameras.
        /// </summary>
        public List<Camera> Cameras
        {
            get { return cameras; }
        }

        /// <summary>
        /// Gets scripts.
        /// </summary>
        public List<Behaviour> Behaviours
        {
            get { return scripts; }
        }

        /// <summary>
        /// Gets prefabs.
        /// </summary>
        public List<GameObject> Prefabs
        {
            get { return prefabs; }
        }

        public List<PostProcessPass> PostProcessPasses
        {
            get { return postProcessPasses; }
        }

        /// <summary>
        /// The root scene object which contains all scene objects.
        /// </summary>
        public Scene()
            : base()
        {
            Name = "SCENE-" + Guid.NewGuid();
            m_Transform.Root = m_Transform;
            sceneObjects = new List<GameObject>();
            m_Scene = this;
            renderList = new List<Renderer>(10);
            materials = new List<Material>(5);
            effects = new List<Effect>(5);
            materialsEffectIndex = new Dictionary<int, int>(5);
            colliders = new List<Collider>(5);
            cameras = new List<Camera>(1);
            scripts = new List<Behaviour>(5);
            lights = new List<Light>(2);
            prefabs = new List<GameObject>();
            postProcessPasses = new List<PostProcessPass>();
            _componentsToDestroy = new List<Component>();
            _needRemoveCheck = false;
            defaultMaterial = new SimpleMaterial(this, "Default Material");
            RenderSettings = new RenderSettings();
            m_PhysicsCollisionSystem = new CollisionSystemSAP();
            m_PhysicsWorld = new World(m_PhysicsCollisionSystem);
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
            m_Initialized = true;

            DefaultMaterial.MainTexture = GraphicsHelper.CreateTexture(Color.AntiqueWhite, 1, 1);

            for (int i = 0, l = materials.Count; i < l; i++)
                materials[i].LoadContent(Application.Content);

            RenderSettings.Skybox.LoadContent(Application.Content);

            UpdateEffectMaterialMatching();

            for (int i = 0; i < sceneObjects.Count; i++)
                sceneObjects[i].Initialize();
        }

        /// <summary>
        /// Update all scene object.
        /// </summary>
        public override void Update()
        {
            base.Update();

            m_PhysicsWorld.Step(Time.DeltaTime, true);

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

            // Second - Check if we need to remove some GameObjectlists.
            //sceneObjects.Check();

            // Third - Safe update
            for (int i = 0; i < sceneObjects.Count; i++)
            {
                if (sceneObjects[i].Enabled)
                    sceneObjects[i].Update();
            }
        }

        /// <summary>
        /// Unload the scene.
        /// </summary>
        public virtual void Unload()
        {
            foreach (Behaviour script in Behaviours)
                script.OnDestroy();

            foreach (GameObject sceneObject in sceneObjects)
                sceneObject.Dispose();

            foreach (Material material in materials)
                material.Dispose();

            foreach (PostProcessPass pass in postProcessPasses)
                pass.Dispose();

            Clear();
            current = null;
        }

        /// <summary>
        /// Clean the scene.
        /// </summary>
        protected void Clear()
        {
            renderList.Clear();
            materials.Clear();
            effects.Clear();
            materialsEffectIndex.Clear();
            colliders.Clear();
            cameras.Clear();
            lights.Clear();
            scripts.Clear();
            sceneObjects.Clear();
            prefabs.Clear();
            postProcessPasses.Clear();
            _componentsToDestroy.Clear();
            _needRemoveCheck = false;
        }

        #endregion

        #region GameObjects/Components management

        public override bool Add(GameObject sceneObject)
        {
            return Add(sceneObject, false);
        }

        public bool Add(GameObject sceneObject, bool noCheck)
        {
            bool canAdd = base.Add(sceneObject);

            if (canAdd)
            {
                if (!sceneObject.IsPrefab)
                {
                    sceneObjects.Add(sceneObject);
                    sceneObject.Scene = this;
                    sceneObject.Transform.Root = m_Transform;

                    if (sceneObject.Enabled)
                    {
                        CheckComponents(sceneObject, ComponentChangeType.Add);
                        sceneObject.PropertyChanged += OnGameObjectPropertyChanged;
                        sceneObject.ComponentChanged += OnGameObjectComponentChanged;
                    }

                    if (m_Initialized && !sceneObject.Initialized)
                        sceneObject.Initialize();
                }
                else
                    AddPrefab(sceneObject);
            }

            return canAdd;
        }

        /// <summary>
        /// Add a prefab only before the scene is started.
        /// </summary>
        /// <param name="prefab"></param>
        protected void AddPrefab(GameObject prefab)
        {
            if (!prefabs.Contains(prefab))
                prefabs.Add(prefab);
        }

        protected void RemovePrefab(GameObject prefab)
        {
            if (prefabs.Contains(prefab))
                prefabs.Remove(prefab);
        }

        /// <summary>
        /// Check all components of a scene object to update all list of the scene.
        /// </summary>
        /// <param name="sceneObject">The scene object.</param>
        /// <param name="type">Type of change.</param>
        protected void CheckComponents(GameObject sceneObject, ComponentChangeType type)
        {
            for (int i = 0; i < sceneObject.Components.Count; i++)
                CheckComponent(sceneObject.Components[i], type);
        }

        /// <summary>
        /// Check a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="type"></param>
        protected void CheckComponent(Component component, ComponentChangeType type)
        {
            if (component is Renderer)
            {
                var renderable = component as Renderer;

                if (type == ComponentChangeType.Add)
                    AddRenderer(renderable);

                else if (type == ComponentChangeType.Remove)
                    RemoveRenderer(renderable);
            }

            else if (component is Behaviour)
            {
                var script = component as Behaviour;

                if (type == ComponentChangeType.Add)
                    AddScript(script);
                else if (type == ComponentChangeType.Remove)
                    RemoveScript(script);
            }

            else if (component is Collider)
            {
                var collider = component as Collider;

                if (type == ComponentChangeType.Add)
                    AddCollider(collider);
                else if (type == ComponentChangeType.Remove)
                    RemoveCollider(collider);
            }

            else if (component is Camera)
            {
                var camera = component as Camera;

                if (type == ComponentChangeType.Add && !cameras.Contains(camera))
                    AddCamera(camera);
                else if (type == ComponentChangeType.Remove)
                    RemoveCamera(camera);
            }

            else if (component is Light)
            {
                var light = component as Light;

                if (type == ComponentChangeType.Add)
                    AddLight(light);
                else if (type == ComponentChangeType.Remove)
                    RemoveLight(light);
            }
        }

        private void OnGameObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Name == "Enabled")
            {
                var sceneObject = (GameObject)sender;
                if (sceneObject.Enabled)
                {
                    CheckComponents(sceneObject, ComponentChangeType.Add);
                    sceneObject.ComponentChanged += OnGameObjectComponentChanged;
                }
                else
                {
                    CheckComponents(sceneObject, ComponentChangeType.Remove);
                    sceneObject.ComponentChanged -= OnGameObjectComponentChanged;
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

        #region Add / Get / Remove materials

        /// <summary>
        /// Add a new material.
        /// </summary>
        /// <param name="material"></param>
        internal protected void AddMaterial(Material material)
        {
            if (!materials.Contains(material))
            {
                materials.Add(material);

                if (m_Initialized)
                {
                    material.LoadContent(Application.Content);

                    if (!effects.Contains(material.m_Effect))
                    {
                        effects.Add(material.m_Effect);
                        materialsEffectIndex.Add(effects.IndexOf(material.m_Effect), materials.IndexOf(material));
                    }
                }
            }
        }

        public int GetMaterialIndexByName(string name)
        {
            for (int i = 0, l = materials.Count; i < l; i++)
                if (materials[i].Name == name)
                    return i;

            return -1;
        }

        /// <summary>
        /// Remove a material.
        /// </summary>
        /// <param name="material"></param>
        internal protected void RemoveMaterial(Material material)
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
                if (!effects.Contains(materials[i].m_Effect))
                {
                    effects.Add(materials[i].m_Effect);
                    materialsEffectIndex.Add(effects.IndexOf(materials[i].m_Effect), materials.IndexOf(materials[i]));
                }
            }
        }

        #endregion

        #region Add/Remove components

        internal protected int AddCamera(Camera camera)
        {
            var index = cameras.IndexOf(camera);

            if (index == -1)
            {
                cameras.Add(camera);
                cameras.Sort();
                index = cameras.Count - 1;

                if (Camera.Main == null)
                    Camera.Main = camera;
            }

            return index;
        }

        protected void AddRenderer(Renderer renderer)
        {
            if (renderList.Contains(renderer))
                return;

            renderList.Add(renderer);
            renderList.Sort();
        }

        protected void AddLight(Light light)
        {
            if (lights.Contains(light))
                return;

            lights.Add(light);
            lights.Sort();
        }

        protected void AddCollider(Collider collider)
        {
            if (!colliders.Contains(collider))
                colliders.Add(collider);
        }

        protected void AddScript(Behaviour script)
        {
            if (!scripts.Contains(script))
                scripts.Add(script);
        }

        protected void RemoveRenderer(Renderer renderable)
        {
            if (renderList.Contains(renderable))
                renderList.Remove(renderable);
        }

        protected void RemoveScript(Behaviour script)
        {
            if (scripts.Contains(script))
                scripts.Remove(script);
        }

        protected void RemoveLight(Light light)
        {
            if (lights.Contains(light))
                lights.Remove(light);
        }

        protected void RemoveCollider(Collider collider)
        {
            if (colliders.Contains(collider))
                colliders.Remove(collider);
        }

        protected void RemoveCamera(Camera camera)
        {
            if (cameras.Contains(camera))
                cameras.Remove(camera);
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

        public override bool Remove(GameObject sceneObject)
        {
            return Remove(sceneObject, false);
        }

        public bool Remove(GameObject sceneObject, bool noCheck = false)
        {
            bool canRemove = base.Remove(sceneObject);

            if (canRemove)
                DestroyObject(sceneObject, noCheck);

            return canRemove;
        }

        public void DestroyObject(GameObject sceneObject, bool noCheck = false)
        {
            for (int i = 0, l = sceneObject.Components.Count; i < l; i++)
                this.DestroyComponent(sceneObject.Components[i]);

            sceneObjects.Remove(sceneObject);
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

        public void Add(PostProcessPass pass)
        {
            if (!postProcessPasses.Contains(pass))
            {
                postProcessPasses.Add(pass);
                pass.Initialize(Application.Content);
            }
        }

        public void Remove(PostProcessPass pass)
        {
            if (postProcessPasses.Contains(pass))
                postProcessPasses.Remove(pass);
        }

        #endregion

        #region Search methods

        public static GameObject FindById(string id)
        {
            if (current != null)
            {
                for (int i = 0; i < current.sceneObjects.Count; i++)
                    if (current.sceneObjects[i].Id == id)
                        return current.sceneObjects[i];
            }
            return null;
        }

        public static GameObject[] FindGameObjectsById(string id)
        {
            var sceneObjects = new List<GameObject>();

            if (current != null)
            {
                for (int i = 0; i < current.sceneObjects.Count; i++)
                    if (current.sceneObjects[i].Id == id)
                        sceneObjects.Add(current.sceneObjects[i]);
            }

            return sceneObjects.ToArray();
        }

        public static T FindObjectOfType<T>() where T : Component
        {
            var scripts = new List<T>();

            if (current != null)
            {
                foreach (GameObject so in current.sceneObjects)
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
                foreach (GameObject so in current.sceneObjects)
                {
                    var components = so.GetComponents<T>();
                    if (components.Length > 0)
                        scripts.AddRange(components);
                }
            }

            return scripts.ToArray();
        }

        public Material GetMaterialById(string id)
        {
            foreach (var mat in materials)
                if (mat.Id == id)
                    return mat;
            return null;
        }

        public Material GetMaterialByName(string name)
        {
            foreach (var mat in materials)
                if (mat.Name == name)
                    return mat;
            return null;
        }

        #endregion

        #region Collisions detection

        public Collider Collides(Collider collider)
        {
            for (int i = 0, l = colliders.Count; i < l; i++)
            {
                if (collider.Collides(colliders[i]))
                    return colliders[i];
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

            for (int i = 0, l = colliders.Count; i < l; i++)
                TestCollision(ref ray, colliders[i], distance, infos);

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
