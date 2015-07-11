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

        /// <summary>
        /// Create an emtpy collider.
        /// </summary>
        public Collider()
            : base()
        {
            IsPickable = true;
            IsTrigger = false;
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
    }
}
