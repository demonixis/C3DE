using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// A component to add a box collider to an object.
    /// </summary>
    public class BoxCollider : Collider
    {
        private BoundingBox _box;

        /// <summary>
        /// Gets the bounding box.
        /// </summary>
        public BoundingBox Box
        {
            get { return _box; }
            set { _box = value; }
        }

        /// <summary>
        /// Create an empty box collider.
        /// </summary>
        public BoxCollider()
            : base()
        {
            _box = new BoundingBox();
        }

        public override void Update()
        {
            if (!sceneObject.IsStatic)
            {

            }
        }

        public override void Compute()
        {
            var renderable = GetComponent<RenderableComponent>();

            if (renderable != null)
                _box = new BoundingBox(Vector3.Zero, new Vector3(renderable.boundingSphere.Radius));
        }

        public override bool Collides(Collider other)
        {
            if (other is SphereCollider)
                return _box.Intersects((other as SphereCollider).Sphere);

            if (other is BoxCollider)
                return _box.Intersects((other as BoxCollider).Box);

            return false;
        }

        public override float? IntersectedBy(ref Ray ray)
        {
            return ray.Intersects(_box);
        }
    }
}
