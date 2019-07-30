using Microsoft.Xna.Framework;

namespace C3DE.Components.Physics
{
    /// <summary>
    /// An abstract class for represent a collider which have a size in the 3D space.
    /// </summary>
    public abstract class Collider : Component
    {
        protected bool _autoCompute;
        protected Vector3 _minimum;
        protected Vector3 _maximum;
        protected Vector3 _center;

        /// <summary>
        /// Allow or not the collider to be picked by a ray cast.
        /// </summary>
        public bool IsPickable { get; set; }

        /// <summary>
        /// Indicates whether the collider a trigger.
        /// </summary>
        public bool IsTrigger { get; set; }

        public Vector3 Center
        {
            get { return _center; }
            set
            {
                _center = value;
                _minimum = _maximum - value;
                _autoCompute = false;
            }
        }

        public Vector3 Size
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                _autoCompute = false;
            }
        }

        public Vector3 Minimum
        {
            get { return _minimum; }
        }

        public Vector3 Maximum
        {
            get { return _maximum; }
        }

        public Rectangle Rectangle
        {
            get => new Rectangle((int)(_center.X - _minimum.X), (int)(_center.Z - _minimum.Z), (int)(_center.X + _minimum.X), (int)(_center.Z + _minimum.Z));
        }

        /// <summary>
        /// Create an emtpy collider.
        /// </summary>
        public Collider()
            : base()
        {
            IsPickable = true;
            IsTrigger = false;
            _center = Vector3.Zero;
            _minimum = Vector3.Zero;
            _maximum = Vector3.Zero;
            _autoCompute = true;
        }

        public override void Start()
        {
            Compute();
        }

        public override void Reset()
        {
            _autoCompute = true;
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

        public void SetSize(Vector3 center, Vector3 min, Vector3 max)
        {
            _center = center;
            _minimum = min;
            _maximum = max;
        }

        public void SetSize(float? x, float? y, float? z)
        {
            if (x.HasValue)
            {
                _maximum.X = x.Value;
                _minimum.X = -x.Value;
            }
            if (y.HasValue)
            {
                _maximum.Y = y.Value;
                _minimum.Y = -y.Value;
            }

            if (z.HasValue)
            {
                _maximum.Z = z.Value;
                _minimum.Z = -z.Value;
            }
        }

        public void SetCenter(float? x, float? y, float? z)
        {
            if (x.HasValue)
                _center.X = x.Value;

            if (y.HasValue)
                _center.Y = y.Value;

            if (z.HasValue)
                _center.Z = z.Value;
        }
    }
}
