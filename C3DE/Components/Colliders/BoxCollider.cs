using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// A component to add a box collider to an object.
    /// </summary>
    [DataContract]
    public class BoxCollider : Collider
    {
        [DataMember]
        private Vector3 _min;

        [DataMember]
        private Vector3 _max;

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
            _min = Vector3.Zero;
            _max = Vector3.Zero;
        }

        public override void Update()
        {
            base.Update();

            _boundingBox.Min = (transform.Position - _min) - Center;
            _boundingBox.Max = _max + Size;
        }

        public override void Compute()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                _min = renderer.boundingBox.Min;
                _max = renderer.boundingBox.Max;
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
