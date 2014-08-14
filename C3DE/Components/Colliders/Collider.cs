using Microsoft.Xna.Framework;

namespace C3DE.Components.Colliders
{
    /// <summary>
    /// An abstract class for represent a collider which have a size in the 3D space.
    /// </summary>
    public abstract class Collider : Component
    {
        private Vector3 _min;
        private Vector3 _max;
        private Vector3 _center;

        /// <summary>
        /// Gets the min size of the collider.
        /// </summary>
        public Vector3 Min
        {
            get { return _min; }
            protected set { _min = value; }
        }

        /// <summary>
        /// Gets the max size of the collider.
        /// </summary>
        public Vector3 Max 
        {
            get { return _max; }
            protected set { _max = value; }
        }

        /// <summary>
        /// Gets the center of the collider.
        /// </summary>
        public Vector3 Center 
        {
            get { return _center; }
            protected set { _center = value; } 
        }

        /// <summary>
        /// Create an emtpy collider.
        /// </summary>
        public Collider()
            : base()
        {
            _min = Vector3.Zero;
            _max = Vector3.Zero;
            _center = Vector3.Zero;
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
