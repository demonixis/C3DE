using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace C3DE.Components
{
    /// <summary>
    /// A component responible to manage transform operations (translation, rotation and scaling).
    /// </summary>
    public class Transform : Component
    {
        internal Matrix world;
        private Vector3 _rotation;
        private Vector3 _position;
        private Vector3 _scale;
        private Transform _parent;
        private Transform _root;
        private bool _dirty;
        protected List<Transform> _transforms;

        public Transform Root
        {
            get { return _root; }
            internal set { _root = value; }
        }

        public Transform Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public List<Transform> Transforms
        {
            get { return _transforms; }
            protected set { _transforms = value; }
        }

        public Vector3 Position
        {
            get { return (_parent != null) ? _parent.Position + _position : _position; }
            set { _position = value; }
        }

        public Vector3 LocalPosition
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3 Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public Vector3 Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public Matrix WorldMatrix
        {
            get { return world; }
        }

        public Transform()
            : this(null)
        {
        }

        public Transform(SceneObject sceneObject)
            : base (sceneObject)
        {
            _position = Vector3.Zero;
            _rotation = Vector3.Zero;
            _scale = Vector3.One;
            _parent = null;
            _root = null;
            _transforms = new List<Transform>();
            _dirty = false;
            world = Matrix.Identity;
        }

        public void Translate(float x, float y, float z)
        {
            _position.X += x;
            _position.Y += y;
            _position.Z += z;
        }

        public void Rotate(float rx, float ry, float rz)
        {
            _rotation.X += rx;
            _rotation.Y += ry;
            _rotation.Z += rz;
        }

        public override void Update()
        {
            if (!sceneObject.IsStatic || _dirty)
            {
                world = Matrix.Identity;
                world *= Matrix.CreateScale(_scale);
                world *= Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);
                world *= Matrix.CreateTranslation(_position);

                if (_parent != null)
                    world *= _parent.world;

                _dirty = false;
            }
        }
    }
}