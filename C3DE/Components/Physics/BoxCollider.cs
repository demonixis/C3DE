using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace C3DE.Components.Physics
{
    /// <summary>
    /// A component to add a box collider to an object.
    /// </summary>
    [DataContract]
    public class BoxCollider : Collider
    {
        [DataMember]
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
            center = Vector3.Zero;
        }

        public override void Update()
        {
            base.Update();
            _boundingBox.Min = (minimum - (center + transform.LocalPosition)) * transform.LocalScale;
            _boundingBox.Max = (maximum + (center + transform.LocalPosition)) * transform.LocalScale;
        }

        public override void Compute()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null && autoCompute)
            {
                minimum = renderer.boundingBox.Min * 2;
                maximum = renderer.boundingBox.Max * 2;
                center = maximum + minimum;
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
