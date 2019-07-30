using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;

namespace C3DE.Components.Physics
{
    /// <summary>
    /// A component to add a box collider to an object.
    /// </summary>
    public class BoxCollider : Collider
    {
        private BoundingBox _boundingBox;

        /// <summary>
        /// Gets the bounding box.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
            protected set { _boundingBox = value; }
        }

        /// <summary>
        /// Create an empty box collider.
        /// </summary>
        public BoxCollider()
            : base()
        {
            _boundingBox = new BoundingBox();
            _center = Vector3.Zero;
        }

        public override void Update()
        {
            base.Update();
            _boundingBox.Min = (_minimum - (_center + _transform.LocalPosition)) * _transform.LocalScale;
            _boundingBox.Max = (_maximum + (_center + _transform.LocalPosition)) * _transform.LocalScale;
        }

        public override void Compute()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null && _autoCompute)
            {
                _minimum = renderer.boundingBox.Min;
                _maximum = renderer.boundingBox.Max;
                _center = _maximum + _minimum;
                Update();
            }
        }

        public override bool Collides(Collider other)
        {
            if (other is SphereCollider)
                return _boundingBox.Intersects((other as SphereCollider).Sphere);

            if (other is BoxCollider)
                return _boundingBox.Intersects((other as BoxCollider).BoundingBox);

            return false;
        }

        public override float? IntersectedBy(ref Ray ray)
        {
            return ray.Intersects(_boundingBox);
        }
    }
}
