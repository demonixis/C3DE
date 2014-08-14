using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;

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
            set { _sphere = value; }
        }

        public SphereCollider()
            : base()
        {
            _sphere = new BoundingSphere();
        }

        public override void Start()
        {
            
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
