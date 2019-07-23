using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace C3DE.Components
{
    /// <summary>
    /// A component responible to manage transform operations (translation, rotation and scaling).
    /// </summary>
    public class Transform : Component
    {
        internal Matrix _worldMatrix;
        private Vector3 _localRotation;
        private Vector3 _localPosition;
        private Vector3 _localScale;
        private Transform _parent;
        private Transform _root;
        private bool _dirty;
        protected List<Transform> _transforms;

        public Transform Root
        {
            get => _root;
            internal set { _root = value; }
        }

        public Transform Parent
        {
            get => _parent;
            set
            {
                if (_parent != null)
                    _parent.Transforms.Remove(this);

                _parent = value;

                if (_parent != null)
                    _parent.Transforms.Add(this);
            }
        }

        public List<Transform> Transforms
        {
            get { return _transforms; }
            protected set { _transforms = value; }
        }

        public Vector3 Position
        {
            get
            {
                UpdateWorldMatrix();
                return _worldMatrix.Translation;
            }
            set
            {
                UpdateWorldMatrix();
                _localPosition = Vector3.Transform(value, Matrix.Invert(_worldMatrix));
            }
        }

        public Vector3 LocalPosition
        {
            get { return _localPosition; }
            set { _localPosition = value; }
        }

        public Vector3 Rotation
        {
            get
            {
                UpdateWorldMatrix();
                var rotation = Quaternion.Identity;
                _worldMatrix.Decompose(out Vector3 scale, out Quaternion r, out Vector3 translation);
                return r.ToEuler();
            }
            set
            {
                var result = Quaternion.Euler(value) * Quaternion.Inverse(_parent.Quaternion);
                _localRotation = result.ToEuler();
            }
        }

        public Matrix RotationMatrix => Matrix.CreateFromYawPitchRoll(_localRotation.Y, _localRotation.X, _localRotation.Z);

        public Quaternion Quaternion
        {
            get
            {
                var rotation = Rotation;
                return Quaternion.Euler(rotation);
            }
        }

        public Quaternion LocalQuaternion => Quaternion.Euler(_localRotation);

        public Vector3 LocalRotation
        {
            get { return _localRotation; }
            set { _localRotation = value; }
        }

        public Vector3 LocalScale
        {
            get { return _localScale; }
            set { _localScale = value; }
        }

        public Matrix WorldMatrix => _worldMatrix;

        public Vector3 Forward => _worldMatrix.Forward;
        public Vector3 Backward => _worldMatrix.Backward;
        public Vector3 Right => _worldMatrix.Right;
        public Vector3 Left => _worldMatrix.Left;
        public Vector3 Up => Vector3.Normalize(Position + Vector3.Transform(Vector3.Up, _worldMatrix));

        public Transform()
            : base()
        {
            _localPosition = Vector3.Zero;
            _localRotation = Vector3.Zero;
            _localScale = Vector3.One;
            _parent = null;
            _root = null;
            _transforms = new List<Transform>();
            _dirty = false;
            _worldMatrix = Matrix.Identity;
        }

        public void Translate(float x, float y, float z)
        {
            _localPosition.X += x;
            _localPosition.Y += y;
            _localPosition.Z += z;
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
            _localRotation.X += rx;
            _localRotation.Y += ry;
            _localRotation.Z += rz;
        }

        public void Rotate(ref Vector3 rotation)
        {
            Rotate(rotation.X, rotation.Y, rotation.Z);
        }

        public void Rotate(Vector3 rotation)
        {
            Rotate(ref rotation);
        }

        public void SetLocalPosition(float? x, float? y, float? z)
        {
            _localPosition.X = x.HasValue ? x.Value : _localPosition.X;
            _localPosition.Y = y.HasValue ? y.Value : _localPosition.Y;
            _localPosition.Z = z.HasValue ? z.Value : _localPosition.Z;
        }

        public void SetLocalPosition(Vector3 position)
        {
            _localPosition.X = position.X;
            _localPosition.Y = position.Y;
            _localPosition.Z = position.Z;
        }

        public void SetLocalRotation(float? x, float? y, float? z)
        {
            _localRotation.X = x.HasValue ? x.Value : _localRotation.X;
            _localRotation.Y = y.HasValue ? y.Value : _localRotation.Y;
            _localRotation.Z = z.HasValue ? z.Value : _localRotation.Z;
        }

        public void SetLocalRotation(Matrix matrix)
        {
            var quaternion = Quaternion.CreateFromRotationMatrix(matrix);
            var rotation = quaternion.ToEuler();
            SetLocalRotation(rotation.X, rotation.Y, rotation.Z);
        }

        public void SetLocalScale(float? x, float? y, float? z)
        {
            _localScale.X = x.HasValue ? x.Value : _localScale.X;
            _localScale.Y = y.HasValue ? y.Value : _localScale.Y;
            _localScale.Z = z.HasValue ? z.Value : _localScale.Z;
        }

        public override void Update()
        {
            if (_gameObject.IsStatic && !_dirty)
                return;

            UpdateWorldMatrix();

            _dirty = false;
        }

        public void UpdateWorldMatrix()
        {
            _worldMatrix = Matrix.Identity;
            _worldMatrix *= Matrix.CreateScale(_localScale);
            _worldMatrix *= Matrix.CreateFromYawPitchRoll(_localRotation.Y, _localRotation.X, _localRotation.Z);
            _worldMatrix *= Matrix.CreateTranslation(_localPosition);

            if (_parent != null)
                _worldMatrix *= _parent._worldMatrix;
        }

        public Vector3 TransformVector(Vector3 direction)
        {
            UpdateWorldMatrix();
            return Vector3.Transform(direction, _worldMatrix);
        }

        public override object Clone()
        {
            var tr = new Transform();
            tr._parent = Parent;
            tr._root = _root;
            tr._gameObject = _gameObject;

            foreach (var t in _transforms)
                tr._transforms.Add(t);

            tr._localPosition = _localPosition;
            tr._localRotation = _localRotation;
            tr._localScale = _localScale;
            tr._dirty = true;

            return tr;
        }
    }
}