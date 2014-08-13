using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// A component to add a box collider to an object.
    /// </summary>
    public class BoxCollider : Collider
    {
        private BoundingBox _box;

        public BoundingBox Box
        {
            get { return _box; }
            set { _box = value; }
        }

        public BoxCollider()
            : base()
        {
            _box = new BoundingBox();
            _center = Vector3.Zero;
        }

        public override void Start()
        {
            Compute();
        }

        /// <summary>
        /// Compute the bounding box.
        /// </summary>
        public override void Update()
        {

        }

        public override void Compute()
        {
            var renderable = GetComponent<RenderableComponent>();

            if (renderable != null)
                _box = BoundingBox.CreateFromSphere(renderable.boundingSphere);
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
