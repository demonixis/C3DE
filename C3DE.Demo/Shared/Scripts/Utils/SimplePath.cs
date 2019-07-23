using C3DE;
using C3DE.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace C3DE.Demo.Scripts.Utils
{
    public class SimplePath : Behaviour
    {
        private List<Vector3> _paths = new List<Vector3>();
        private List<Vector3> _memPaths;
        private bool _beginStarted;

        public bool Loop { get; set; } = true;
        public float MoveSpeed { get; set; } = 2.5f;
        public float RotationSpeed { get; set; } = 2.5f;
        public bool UpdateRotation { get; set; } = true;

        public bool IsDone => _paths.Count == 0;

        public event EventHandler<EventArgs> PathDone = null;

        public SimplePath()
            : base()
        {
            _beginStarted = false;
            _memPaths = new List<Vector3>();
        }

        // Update is called once per frame
        public override void Update()
        {
            if (!_beginStarted && _paths.Count > 0 && MoveToPoint(Time.DeltaTime, _paths[0]))
            {
                _paths.RemoveAt(0);

                if (_paths.Count == 0)
                {
                    PathDone?.Invoke(this, EventArgs.Empty);

                    if (Loop)
                    {
                        _memPaths.Reverse();
                        _paths = _memPaths;
                    }
                }
            }
        }

        public void Begin()
        {
            _paths.Clear();
            _beginStarted = true;
        }

        public void AddPath(Vector3 path)
        {
            _paths.Add(path);
        }

        public void AddPath(Vector3 path, Transform transform)
        {
            _paths.Add(transform.LocalPosition + path);
        }

        public void End()
        {
            if (_beginStarted)
            {
                _beginStarted = false;
                _memPaths = new List<Vector3>(_paths);
            }
        }

        public void Clear()
        {
            _memPaths.Clear();
            _paths.Clear();
        }

        private bool MoveToPoint(float elapsedTime, Vector3 target)
        {
            if (_transform.LocalPosition == target)
                return true;

            var direction = Vector3.Normalize(target - _transform.Position);
            _transform.LocalPosition += direction * MoveSpeed * elapsedTime;

            if (UpdateRotation)
            {
                var mat = Matrix.CreateLookAt(_transform.Position, direction, Vector3.Up);
                mat.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
                var euler = rotation.ToEuler();
                euler.X = MathHelper.ToRadians(euler.X);
                euler.Y = MathHelper.ToRadians(euler.Y);
                euler.Z = MathHelper.ToRadians(euler.Z);
                _transform.LocalRotation = Vector3.Lerp(_transform.LocalRotation, euler, RotationSpeed * Time.DeltaTime);
            }

            if (Math.Abs(Vector3.Dot(direction, Vector3.Normalize(target - _transform.LocalPosition)) + 1) < 0.1f)
                _transform.LocalPosition = target;

            return _transform.LocalPosition == target;
        }
    }
}