using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// A component to add a box collider to an object.
    /// </summary>
    public class BoxCollider : Component
    {
        private RenderableComponent _renderable;
        private BoundingBox _box;
        private Vector3 _center;
        private bool _dirty;

        public BoundingBox Box
        {
            get {return _box;}
        }

        public Vector3 Min
        {
            get { return _box.Min; }
        }

        public Vector3 Max
        {
            get { return _box.Max; }
        }

        public Vector3 Center
        {
            get { return _center; }
        }

        public BoxCollider()
            : this (null)
        {
        }

        public BoxCollider(SceneObject sceneObject)
            : base(sceneObject)
        {
            _box = new BoundingBox();
            _center = Vector3.Zero;
            _dirty = true;
        }

        public override void LoadContent(ContentManager content)
        {
            _renderable = GetComponent<RenderableComponent>();
        }

        /// <summary>
        /// Compute the bounding box.
        /// </summary>
        public override void Update()
        {
            if (_dirty)
            {
                _dirty = false;

                var sphere = _renderable.GetBoundingSphere();
                _box = BoundingBox.CreateFromSphere(sphere);
                _center = sphere.Center;
            }
        }
    }
}
