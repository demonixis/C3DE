using C3DE.Components;
using C3DE.Components.Cameras;
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
        public BoxCollider Collider;
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
        private List<BoxCollider> _colliders; // FIXME
        private List<Camera> _cameras;
        private int _mainCameraIndex;
        private List<Light> _lights;

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

        public List<BoxCollider> Colliders
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
            _renderList = new List<RenderableComponent>();
            _materials = new List<Material>();
            _colliders = new List<BoxCollider>();
            _cameras = new List<Camera>();
            _mainCameraIndex = -1;
            _lights = new List<Light>();
            DefaultMaterial = new Material(this);
        }

        #region Lifecycle

        /// <summary>
        /// Load content of all components.
        /// </summary>
        /// <param name="content"></param>
        public override void LoadContent(ContentManager content)
        {
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

            // Check if lists needs to be updated (components waiting to be added or removed)
            members.Check();

            // Safe update
            for (int i = 0; i < members.Size; i++)
            {
                if (members[i].Enabled)
                    members[i].Update();
            }
        }

        #endregion

        #region SceneObject collection management

        /// <summary>
        /// Check all components of a scene object to update all list of the scene.
        /// </summary>
        /// <param name="sceneObject">The scene object.</param>
        /// <param name="type">Type of change.</param>
        private void CheckComponents(SceneObject sceneObject, ComponentChangeType type)
        {
            for (int i = 0; i < sceneObject.Components.Size; i++)
                CheckComponent(sceneObject.Components[i], type);
        }

        public void CheckComponent(Component component, ComponentChangeType type)
        {
            if (component is Camera)
            {
                if (type == ComponentChangeType.Add)
                    Add(component as Camera);
                else if (type == ComponentChangeType.Remove)
                    Remove(component as Camera);
            }
            
            else if (component is RenderableComponent)
            {
                var renderable = component as RenderableComponent;

                if (type == ComponentChangeType.Add && !_renderList.Contains(renderable))
                    _renderList.Add(renderable);

                else if (type == ComponentChangeType.Remove)
                    _renderList.Remove(renderable);
            }

            else if (component is BoxCollider)
            {
                var collider = component as BoxCollider;

                if (collider != null)
                {
                    if (type == ComponentChangeType.Add && !_colliders.Contains(collider))
                        _colliders.Add(collider);
                    else if (type == ComponentChangeType.Remove)
                        _colliders.Remove(collider);
                }
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

        /// <summary>
        /// Remove a scene object from the scene.
        /// </summary>
        /// <param name="sceneObject">The scene object to remove.</param>
        /// <returns>Return true if the component has been removed, otherwise return false.</returns>
        public override bool Remove(SceneObject sceneObject)
        {
            bool canRemove = base.Remove(sceneObject);
            int index = -1;

            if (canRemove)
            {
                index = members.IndexOf(sceneObject);

                if (index > -1)
                {
                    sceneObject.ComponentChanged -= sceneObject_ComponentsChanged;

                    CheckComponents(sceneObject, ComponentChangeType.Remove);

                    members.Remove(sceneObject);
                    sceneObject.Scene = null;
                    sceneObject.Transform.Root = null;
                }
            }

            return canRemove && (index > -1);
        }

        #endregion

        #region Material collection management

        /// <summary>
        /// Add a new material.
        /// </summary>
        /// <param name="material"></param>
        public void Add(Material material)
        {
            if (!_materials.Contains(material))
                _materials.Add(material);
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

        #region Camera collection management

        public int Add(Camera camera)
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

        public void Remove(Camera camera)
        {
            if (_cameras.Contains(camera))
                _cameras.Remove(camera);
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
                val = ray.Intersects(_colliders[i].Box);

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
                val = ray.Intersects(_colliders[i].Box);

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
