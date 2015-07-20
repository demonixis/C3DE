using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// An abstract class for represent a collider which have a size in the 3D space.
    /// </summary>
    [DataContract]
    public abstract class Collider : Component
    {
        [DataMember]
        protected Vector3 center;

        [DataMember]
        protected Vector3 size;

        /// <summary>
        /// Allow or not the collider to be picked by a ray cast.
        /// </summary>
        [DataMember]
        public bool IsPickable { get; set; }

        /// <summary>
        /// Indicates whether the collider a trigger.
        /// </summary>
        [DataMember]
        public bool IsTrigger { get; set; }

        public Vector3 Center
        {
            get { return center; }
            set { center = value; }
        }

        public Vector3 Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// Create an emtpy collider.
        /// </summary>
        public Collider()
            : base()
        {
            IsPickable = true;
            IsTrigger = false;
            center = Vector3.Zero;
            size = Vector3.One;
        }

        public override void Start()
        {
            Compute();
        }

        /// <summary>
        /// Compute the collider if a renderable component is attached to the scene object.
        /// This method can be called by a renderable component if needed.
        /// </summary>
        public abstract void Compute();

        /// <summary>
        /// Check if the collider enter in collision with another collider.
        /// </summary>
        /// <param name="other">A collider</param>
        /// <returns>Returns true if it collides, otherwise it returns false.</returns>
        public abstract bool Collides(Collider other);

        /// <summary>
        /// Check if the collider is intersected by a ray.
        /// </summary>
        /// <param name="ray">A ray</param>
        /// <returns>Returns true if it collides, otherwise it returns false.</returns>
        public abstract float? IntersectedBy(ref Ray ray);

        public void SetSize(float? x, float? y, float? z)
        {
            if (x.HasValue)
                size.X = x.Value;

            if (y.HasValue)
                size.Y = y.Value;

            if (z.HasValue)
                size.Z = z.Value;
        }

        public void SetCenter(float? x, float? y, float? z)
        {
            if (x.HasValue)
                center.X = x.Value;

            if (y.HasValue)
                center.Y = y.Value;

            if (z.HasValue)
                center.Z = z.Value;
        }
    }
}
