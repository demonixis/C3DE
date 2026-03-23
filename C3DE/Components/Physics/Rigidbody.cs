using System;
using C3DE.Components.Rendering;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Microsoft.Xna.Framework;
using JRigidBody = Jitter2.Dynamics.RigidBody;

namespace C3DE.Components.Physics
{
    public class Rigidbody : Component
    {
        private JRigidBody _rigidBody;

        public bool IsStatic
        {
            get => _rigidBody?.MotionType == MotionType.Static;
            set
            {
                if (_rigidBody != null)
                    _rigidBody.MotionType = value ? MotionType.Static : (IsKinematic ? MotionType.Kinematic : MotionType.Dynamic);
            }
        }

        public bool Gravity
        {
            get => _rigidBody?.AffectedByGravity ?? true;
            set { if (_rigidBody != null) _rigidBody.AffectedByGravity = value; }
        }

        public Vector3 AngularVelocity
        {
            get => _rigidBody != null ? ToVector3(_rigidBody.AngularVelocity) : Vector3.Zero;
            set { if (_rigidBody != null) _rigidBody.AngularVelocity = ToJVector(value); }
        }

        public Vector3 Velocity
        {
            get => _rigidBody != null ? ToVector3(_rigidBody.Velocity) : Vector3.Zero;
            set { if (_rigidBody != null) _rigidBody.Velocity = ToJVector(value); }
        }

        public float Mass => _rigidBody?.Mass ?? 1.0f;

        public bool IsKinematic { get; set; } = false;

        public JRigidBody JitterRigidbody => _rigidBody;

        public RigidBodyShape Shape => _rigidBody?.Shapes.Count > 0 ? _rigidBody.Shapes[0] : null;

        public Rigidbody() : base() { }

        public override void Start()
        {
            base.Start();

            var renderer = GameObject.GetComponentInChildren<Renderer>();
            var collider = GameObject.GetComponentInChildren<Collider>();

            if (collider != null)
                SetShapeFromCollider(collider);
            else if (renderer != null)
                SetShapeFromRenderer(renderer);
        }

        public void SyncTransform()
        {
            if (_rigidBody == null) return;
            var rot = _transform.LocalRotation;
            var q = Quaternion.CreateFromYawPitchRoll(rot.Y, rot.X, rot.Z);
            _rigidBody.Position = ToJVector(_transform.LocalPosition);
            _rigidBody.Orientation = new JQuaternion(q.X, q.Y, q.Z, q.W);
        }

        public void SetShape(RigidBodyShape shape)
        {
            if (Scene.current == null) return;

            if (_rigidBody == null)
            {
                _rigidBody = Scene.current._physicsWorld.CreateRigidBody();
            }
            else
            {
                while (_rigidBody.Shapes.Count > 0)
                    _rigidBody.RemoveShape(_rigidBody.Shapes[0]);
            }

            _rigidBody.AddShape(shape);
            SyncTransform();
        }

        public void SetShapeFromCollider(Collider collider)
        {
            RigidBodyShape shape;
            var size = collider.Size;
            var sphere = collider as SphereCollider;

            if (sphere != null)
                shape = new SphereShape(sphere.Sphere.Radius);
            else
            {
                const float minDim = 0.001f;
                shape = new BoxShape(Math.Max(size.X * 2, minDim), Math.Max(size.Y * 2, minDim), Math.Max(size.Z * 2, minDim));
            }

            SetShape(shape);
        }

        public void SetShapeFromRenderer(Renderer renderer)
        {
            SetShape(new SphereShape(renderer.boundingSphere.Radius));
        }

        public void AddForce(Vector3 force)
        {
            _rigidBody?.AddForce(ToJVector(force));
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position)
        {
            _rigidBody?.AddForce(ToJVector(force), ToJVector(position));
        }

        public void AddTorque(Vector3 torque)
        {
            // Jitter2 does not expose AddTorque on RigidBody directly.
            // Apply torque as a force pair if needed in the future.
        }

        public override void Update()
        {
            base.Update();

            if (_rigidBody == null)
                return;

            if (!IsKinematic)
                _transform.SetLocalPositionAndRotation(ToVector3(_rigidBody.Position), ToMatrix(_rigidBody.Orientation));
            else
                SyncTransform();
        }

        public override void Dispose()
        {
            if (_rigidBody != null)
            {
                Scene.current?._physicsWorld.Remove(_rigidBody);
                _rigidBody = null;
            }
        }

        public static Vector3 ToVector3(JVector jVector) => new Vector3(jVector.X, jVector.Y, jVector.Z);
        public static JVector ToJVector(Vector3 vector) => new JVector(vector.X, vector.Y, vector.Z);

        public static Matrix ToMatrix(JQuaternion q)
        {
            return Matrix.CreateFromQuaternion(new Quaternion(q.X, q.Y, q.Z, q.W));
        }

        public static JQuaternion ToJQuaternion(Matrix matrix)
        {
            var q = Quaternion.CreateFromRotationMatrix(matrix);
            return new JQuaternion(q.X, q.Y, q.Z, q.W);
        }
    }
}
