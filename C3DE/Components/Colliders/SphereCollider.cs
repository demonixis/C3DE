using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// A component to add a box collider to an object.
    /// </summary>
    public class SphereCollider : Collider
    {
        private BoundingSphere _sphere;

        public BoundingSphere Sphere
        {
            get { return _sphere; }
        }

        public SphereCollider()
            : this(null)
        {
        }

        public SphereCollider(SceneObject sceneObject)
            : base(sceneObject)
        {
            _sphere = new BoundingSphere();
        }

        /// <summary>
        /// Compute the bounding box.
        /// </summary>
        public override void Update()
        {

        }

        public override bool Collides(Collider other)
        {
            var sc = other as SphereCollider;
            if (sc != null)
                return sc.Sphere.Intersects(_sphere);

            var bc = other as BoxCollider;
            if (bc != null)
                return bc.Box.Intersects(_sphere);

            return false;
        }

        public override float? IntersectedBy(ref Ray ray)
        {
            return ray.Intersects(_sphere);
        }
    }
}
