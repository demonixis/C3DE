using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// A component to add a box collider to an object.
    /// </summary>
    public class BoxCollider : Collider
    {
        private RenderableComponent _renderable;
        private BoundingBox _box;

        public BoundingBox Box
        {
            get {return _box;}
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
            if (dirty)
            {
                dirty = false;

                var sphere = _renderable.GetBoundingSphere();
                _box = BoundingBox.CreateFromSphere(sphere);
                _center = sphere.Center;
            }
        }

        public override bool Collides(Collider other)
        {
            var sc = other as SphereCollider;
            if (sc != null)
                return sc.Sphere.Intersects(_box);

            var bc = other as BoxCollider;
            if (bc != null)
                return bc.Box.Intersects(_box);

            return false;
        }

        public override float? IntersectedBy(ref Ray ray)
        {
            return ray.Intersects(_box);
        }
    }
}
