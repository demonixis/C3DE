using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// A sphere collider component used to handle collisions by sphere.
    /// </summary>
    public class SphereCollider : Collider
    {
        private BoundingSphere _sphere;

        /// <summary>
        /// Gets the bounding sphere.
        /// </summary>
        public BoundingSphere Sphere
        {
            get { return _sphere; }
            set { _sphere = value; }
        }

        /// <summary>
        /// Create an empty sphere collider.
        /// </summary>
        public SphereCollider()
            : base()
        {
            _sphere = new BoundingSphere();
        }

        public override void Update()
        {
            if (!sceneObject.IsStatic)
                _sphere.Center = transform.Position;
        }

        public override void Compute()
        {
            var renderable = GetComponent<RenderableComponent>();

            if (renderable != null)
            {
                _sphere = renderable.boundingSphere;
                Min = new Vector3(-_sphere.Radius);
                Max = new Vector3(_sphere.Radius);
            }
        }

        public override bool Collides(Collider other)
        {
            if (other is SphereCollider)
                return (other as SphereCollider).Sphere.Intersects(_sphere);

            if (other is BoxCollider)
                return (other as BoxCollider).Box.Intersects(_sphere);

            return false;
        }

        public override float? IntersectedBy(ref Ray ray)
        {
            return ray.Intersects(_sphere);
        }
    }
}
