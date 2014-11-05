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
            get { return _position; }
            set { _position = value; }
        }

        public Vector3 Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public Vector3 LocalScale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public Matrix WorldMatrix
        {
            get { return world; }
        }
		
		public Vector3 Forward
		{
			get
			{	
				return getTransformedVector(Vector3.Forward);
			}
		}

		public Vector3 Backward
		{
			get
			{	
				return getTransformedVector(Vector3.Backward);
			}
		}

		public Vector3 Right
		{
			get
			{	
				return getTransformedVector(Vector3.Right);
			}
		}

		public Vector3 Left
		{
			get
			{	
				return getTransformedVector(Vector3.Left);
			}
		}

        public Transform()
            : base ()
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

        public void Translate(ref Vector3 translation)
        {
            Translate(translation.X, translation.Y, translation.Z);
        }

        public void Translate(Vector3 translation)
        {
            Translate(ref translation);
        }

        public void Rotate(float rx, float ry, float rz)
        {
            _rotation.X += rx;
            _rotation.Y += ry;
            _rotation.Z += rz;
        }

        public void Rotate(ref Vector3 rotation)
        {
            Rotate(rotation.X, rotation.Y, rotation.Z);
        }

        public void Rotate(Vector3 rotation)
        {
            Rotate(ref rotation);
        }

        public void SetPosition(float? x, float? y, float? z)
        {
            _position.X = x.HasValue ? x.Value : _position.X;
            _position.Y = y.HasValue ? y.Value : _position.Y;
            _position.Z = z.HasValue ? z.Value : _position.Z;
        }

        public void SetRotation(float? x, float? y, float ?z)
        {
            _rotation.X = x.HasValue ? x.Value : _rotation.X;
            _rotation.Y = y.HasValue ? y.Value : _rotation.Y;
            _rotation.Z = z.HasValue ? z.Value : _rotation.Z;
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
		
		private Vector3 getTransformedVector(Vector3 direction)
		{
			return Vector3.Transform(direction, Matrix.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z));
		}
    }
}