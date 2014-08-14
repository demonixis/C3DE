using Microsoft.Xna.Framework;

namespace C3DE.Components.Colliders
{
    public abstract class Collider : Component
    {
        private Vector3 _min;
        private Vector3 _max;
        private Vector3 _center;

        public Vector3 Min
        {
            get { return _min; }
            protected set { _min = value; }
        }

        public Vector3 Max 
        {
            get { return _max; }
            protected set { _max = value; }
        }

        public Vector3 Center 
        {
            get { return _center; }
            protected set { _center = value; } 
        }

        public Collider()
            : base()
        {
            _min = Vector3.Zero;
            _max = Vector3.Zero;
            _center = Vector3.Zero;
        }

        public override void Update()
        {
            if (!sceneObject.IsStatic)
                _center = transform.Position;
        }

        public abstract void Compute();

        public abstract bool Collides(Collider other);

        public abstract float? IntersectedBy(ref Ray ray);
    }
}
