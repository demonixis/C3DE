using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace C3DE.Components.Physics
{
    /// <summary>
    /// An abstract class for represent a collider which have a size in the 3D space.
    /// </summary>
    [DataContract]
    public abstract class Collider : Component
    {
        [DataMember]
        protected bool autoCompute;

        [DataMember]
        protected Vector3 minimum;

        [DataMember]
        protected Vector3 maximum;

        [DataMember]
        protected Vector3 center;

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
            set
            {
                center = value;
                minimum = maximum - value;
                autoCompute = false;
            }
        }

        public Vector3 Size
        {
            get { return maximum; }
            set
            {
                maximum = value;
                autoCompute = false;
            }
        }

        public Vector3 Minimum
        {
            get { return minimum; }
        }

        public Vector3 Maximum
        {
            get { return maximum; }
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
            minimum = Vector3.Zero;
            maximum = Vector3.Zero;
            autoCompute = true;
        }

        public override void Start()
        {
            Compute();
        }

        public override void Reset()
        {
            autoCompute = true;
            Update();
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
            {
                maximum.X = x.Value;
                minimum.X = -x.Value;
            }
            if (y.HasValue)
            {
                maximum.Y = y.Value;
                minimum.Y = -y.Value;
            }

            if (z.HasValue)
            {
                maximum.Z = z.Value;
                minimum.Z = -z.Value;
            }
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
