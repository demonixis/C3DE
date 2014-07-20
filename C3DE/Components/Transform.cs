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
            world = Matrix.Identity;
        }

        public void Translate(float x, float y, float z)
        {
            Vector3 move = new Vector3(x, y, z);
            Matrix forwardMovement = Matrix.CreateRotationY(_rotation.Y);
            Vector3 tVector = Vector3.Transform(move, forwardMovement);
            _position.X += tVector.X;
            _position.Y += tVector.Y;
            _position.Z += tVector.Z;
        }

        public void Rotate(float rx, float ry, float rz)
        {
            _rotation.X += rx;
            _rotation.Y += ry;
            _rotation.Z += rz;
        }

        public override void Update()
        {
            world = Matrix.Identity * Matrix.CreateScale(_scale);

            // If a parent exists
            if (_parent != null)
            {
                world *= _parent.world;
                world *= Matrix.CreateFromAxisAngle(_parent.world.Right, _rotation.X);
                world *= Matrix.CreateFromAxisAngle(_parent.world.Up, _rotation.Y);
                world *= Matrix.CreateFromAxisAngle(_parent.world.Forward, _rotation.Z);
            }
            // Local transforms
            else
                world *= Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);

            world *= Matrix.CreateTranslation(_position);
        }
    }
}