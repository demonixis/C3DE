using Microsoft.Xna.Framework;

namespace C3DE.Components.Colliders
{
    public abstract class Collider : Component
    {
        protected Vector3 _min;
        protected Vector3 _max;
        protected Vector3 _center;
        protected bool dirty;

        public Vector3 Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public Vector3 Max 
        {
            get { return _max; }
            set { _max = value; }
        }

        public Vector3 Center 
        {
            get { return _center; }
            set { _center = value; } 
        }

        public Collider()
            : this(null)
        {
        }

        public Collider(SceneObject sceneObject)
            : base(sceneObject)
        {
            _min = Vector3.Zero;
            _max = Vector3.Zero;
            _center = Vector3.Zero;
            dirty = true;
        }

        public abstract bool Collides(Collider other);

        public abstract float? IntersectedBy(ref Ray ray);
    }
}
