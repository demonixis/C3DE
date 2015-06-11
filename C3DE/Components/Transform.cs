﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace C3DE.Components
{
    /// <summary>
    /// A component responible to manage transform operations (translation, rotation and scaling).
    /// </summary>
    public class Transform : Component
    {
        internal Matrix world;
        internal Vector3 lastPosition;
        private Vector3 _localRotation;
        private Vector3 _localPosition;
        private Vector3 _localScale;
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
            get { return _localPosition; }
            set { _localPosition = value; }
        }

        public Vector3 Rotation
        {
            get { return _localRotation; }
            set { _localRotation = value; }
        }

        public Vector3 LocalScale
        {
            get { return _localScale; }
            set { _localScale = value; }
        }

        public Matrix WorldMatrix
        {
            get { return world; }
        }

        public Vector3 Forward
        {
            get { return GetTransformedVector(Vector3.Forward); }
        }

        public Vector3 Backward
        {
            get { return GetTransformedVector(Vector3.Backward); }
        }

        public Vector3 Right
        {
            get { return GetTransformedVector(Vector3.Right); }
        }

        public Vector3 Left
        {
            get { return GetTransformedVector(Vector3.Left); }
        }

        public Vector3 Up
        {
            get { return GetTransformedVector(Vector3.Up); }
        }

        public Vector3 Down
        {
            get { return GetTransformedVector(Vector3.Down); }
        }

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
            world = Matrix.Identity;
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

        public void SetPosition(float? x, float? y, float? z)
        {
            _localPosition.X = x.HasValue ? x.Value : _localPosition.X;
            _localPosition.Y = y.HasValue ? y.Value : _localPosition.Y;
            _localPosition.Z = z.HasValue ? z.Value : _localPosition.Z;
        }

        public void SetRotation(float? x, float? y, float? z)
        {
            _localRotation.X = x.HasValue ? x.Value : _localRotation.X;
            _localRotation.Y = y.HasValue ? y.Value : _localRotation.Y;
            _localRotation.Z = z.HasValue ? z.Value : _localRotation.Z;
        }

        public override void Update()
        {
            if (!sceneObject.IsStatic || _dirty)
            {
                lastPosition = Position;

                world = Matrix.Identity;
                world *= Matrix.CreateScale(_localScale);
                world *= Matrix.CreateFromYawPitchRoll(_localRotation.Y, _localRotation.X, _localRotation.Z);
                world *= Matrix.CreateTranslation(_localPosition);

                if (_parent != null)
                    world *= _parent.world;

                _dirty = false;
            }
        }

        private Vector3 GetTransformedVector(Vector3 direction)
        {
            return Vector3.Transform(direction, Matrix.CreateFromYawPitchRoll(_localRotation.Y, _localRotation.X, _localRotation.Z));
        }
    }
}