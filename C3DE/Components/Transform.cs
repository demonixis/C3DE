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

                _dirty = true;
            }
        }

        public List<Transform> Transforms => _transforms;

        public bool Dirty
        {
            get => _dirty;
            set
            {
                if (!_dirty)
                    _dirty = value;
            }
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
                _dirty = true;
            }
        }

        public Vector3 LocalPosition
        {
            get { return _localPosition; }
            set
            {
                _localPosition = value;
                _dirty = true;
            }
        }

        public Vector3 Rotation
        {
            get
            {
                UpdateWorldMatrix();
                _worldMatrix.Decompose(out Vector3 scale, out Quaternion r, out Vector3 translation);
                return r.ToEuler();
            }
            set
            {
                _localRotation = value;
                _dirty = true; // FIXME
            }
        }

        public Quaternion Quaternion
        {
            get
            {
                UpdateWorldMatrix();
                _worldMatrix.Decompose(out Vector3 scale, out Quaternion r, out Vector3 translation);
                return r;
            }
            set
            {
                value.ToEuler(ref _localRotation);
            }
        }

        public Matrix RotationMatrix
        {
            get => Matrix.CreateFromYawPitchRoll(_localRotation.Y, _localRotation.X, _localRotation.Z);
        }

        public Vector3 LocalRotation
        {
            get { return _localRotation; }
            set
            {
                _localRotation = value;
                _dirty = true;
            }
        }

        public Vector3 LocalScale
        {
            get { return _localScale; }
            set
            {
                _localScale = value;
                _dirty = true;
            }
        }

        #region Unity Interop

        public Transform parent
        {
            get => Parent;
            set => Parent = value;
        }

        /// <summary>
        /// Unity Interop
        /// </summary>
        public Vector3 position
        {
            get => Position;
            set => Position = value;
        }

        public Vector3 localPosition
        {
            get => LocalPosition;
            set => LocalPosition = value;
        }

        public Quaternion rotation
        {
            get
            {
                UpdateWorldMatrix();
                _worldMatrix.Decompose(out Vector3 scale, out Quaternion r, out Vector3 translation);
                return r;
            }
            set
            {
                value.ToEuler(ref _localRotation);
            }
        }

        public Quaternion localRotation
        {
            get
            {
                return Quaternion.CreateFromYawPitchRoll(_localRotation.Y, _localRotation.X, _localRotation.Z);
            }
            set
            {
                value.ToEuler(ref _localRotation);
            }
        }

        public Vector3 localScale
        {
            get => LocalScale;
            set => LocalScale = value;
        }

        #endregion

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

        public void SetParent(Transform parent, bool worldPos)
        {
            Parent = parent;
            _dirty = true;
        }

        public Transform GetChild(int index)
        {
            if (index < _transforms.Count)
                return _transforms[index];

            return null;
        }

        public void Translate(float x, float y, float z)
        {
            _localPosition.X += x;
            _localPosition.Y += y;
            _localPosition.Z += z;
            _dirty = true;
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
            _dirty = true;
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
            _dirty = true;
        }

        public void SetLocalPosition(Vector3 position)
        {
            _localPosition.X = position.X;
            _localPosition.Y = position.Y;
            _localPosition.Z = position.Z;
            _dirty = true;
        }

        public void SetLocalRotation(float? x, float? y, float? z)
        {
            _localRotation.X = x.HasValue ? x.Value : _localRotation.X;
            _localRotation.Y = y.HasValue ? y.Value : _localRotation.Y;
            _localRotation.Z = z.HasValue ? z.Value : _localRotation.Z;
            _dirty = true;
        }

        public void SetLocalPositionAndRotation(Vector3 position, Matrix matrix)
        {
            var quaternion = Quaternion.CreateFromRotationMatrix(matrix);
            var rotation = quaternion.ToEuler();
            
            _localPosition = position;
            _localRotation = rotation;
            _dirty = true;
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
            _dirty = true;
        }

        public override void Update()
        {
            if (_dirty || !_gameObject.IsStatic)
            {
                UpdateWorldMatrix();
                _dirty = false;
            }
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