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
                _box.Min = transform.Position * transform.LocalScale;
                _box.Max = (transform.Position + Max) * transform.LocalScale;
            }
        }

        public override void Compute()
        {
            var renderable = GetComponent<RenderableComponent>();

            if (renderable != null)
            {
                _box = BoundingBox.CreateFromSphere(renderable.boundingSphere);
                Min = _box.Min;
                Max = _box.Max;
            }
        }

        public override bool Collides(Collider other)
        {
            if (other is SphereCollider)
                return (other as SphereCollider).Sphere.Intersects(_box);

            if (other is BoxCollider)
                return (other as BoxCollider).Box.Intersects(_box);

            return false;
        }

        public override float? IntersectedBy(ref Ray ray)
        {
            return ray.Intersects(_box);
        }
    }
}
