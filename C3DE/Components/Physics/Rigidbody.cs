using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using Jitter.Collision.Shapes;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Microsoft.Xna.Framework;
using JRigidBody = Jitter.Dynamics.RigidBody;

namespace C3DE.Components.Physics
{
    public class Rigidbody : Component
    {
        private JRigidBody _rigidBody;
        private bool _addedToScene;

        public bool IsStatic
        {
            get => _rigidBody.IsStatic;
            set => _rigidBody.IsStatic = value;
        }

        public bool AffectedByGravity
        {
            get => _rigidBody?.AffectedByGravity ?? false;
            set => _rigidBody.AffectedByGravity = value;
        }

        public Vector3 AngularVelocity
        {
            get => ToVector3(_rigidBody.AngularVelocity);
            set => _rigidBody.AngularVelocity = ToJVector(value);
        }

        public Vector3 Velocity
        {
            get => ToVector3(_rigidBody.LinearVelocity);
            set => _rigidBody.LinearVelocity = ToJVector(value);
        }

        public Material PhysicsMaterial
        {
            get => _rigidBody.Material;
            set => _rigidBody.Material = value;
        }

        public float Mass
        {
            get => _rigidBody.Mass;
            set => _rigidBody.Mass = value;
        }

        public bool IsKinematic { get; set; } = false;

        public JRigidBody JitterRigidbody
        {
            get => _rigidBody;
        }

        public Shape Shape => _rigidBody.Shape;

        public Rigidbody()
            : base()
        {
            var shape = new BoxShape(1.0f, 1.0f, 1.0f);
            _rigidBody = new JRigidBody(shape);
        }

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
            _rigidBody.Position = ToJVector(_transform.LocalPosition);
            _rigidBody.Orientation = ToJMatrix(Matrix.CreateFromYawPitchRoll(_transform.LocalRotation.Y, _transform.LocalRotation.Y, _transform.LocalRotation.Z));
        }

        public void SetShape(Shape shape)
        {
            _rigidBody.Shape = shape;

            SyncTransform();
            shape.UpdateShape();

            if (!_addedToScene && Scene.current != null)
            {
                Scene.current._physicsWorld.AddBody(_rigidBody);
                _addedToScene = true;
            }
        }

        public void SetShapeFromCollider(Collider collider)
        {
            var shape = (Shape)null;
            var size = collider.Size;
            var sphere = collider as SphereCollider;

            if (sphere != null)
                shape = new SphereShape(sphere.Sphere.Radius);
            else
                shape = new BoxShape(size.X  * 2, size.Y * 2, size.Z * 2);

            SetShape(shape);
        }

        public void SetShapeFromRenderer(Renderer renderer)
        {
            var shape = new SphereShape(renderer.boundingSphere.Radius);
            SetShape(shape);
        }

        public void AddForce(Vector3 force)
        {
            _rigidBody.AddForce(ToJVector(force));
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position)
        {
            _rigidBody.AddForce(ToJVector(force), ToJVector(position));
        }

        public void AddTorque(Vector3 torque)
        {
            _rigidBody.AddTorque(ToJVector(torque));
        }

        public override void Update()
        {
            base.Update();

            if (!_addedToScene)
                return;

            if (!IsKinematic)
            {
                _transform.SetLocalPosition(ToVector3(_rigidBody.Position));
                _transform.SetLocalRotation(ToMatrix(_rigidBody.Orientation));
            }
            else
                SyncTransform();
        }

        public override void Dispose()
        {
            if (_rigidBody != null)
                Scene.current?._physicsWorld.RemoveBody(_rigidBody);
        }

        public static Vector3 ToVector3(JVector jVector) => new Vector3(jVector.X, jVector.Y, jVector.Z);
        public static JVector ToJVector(Vector3 vector) => new JVector(vector.X, vector.Y, vector.Z);

        public static Matrix ToMatrix(JMatrix mat)
        {
            return new Matrix(
                mat.M11, mat.M12, mat.M13, 0.0f,
                mat.M21, mat.M22, mat.M23, 0.0f,
                mat.M31, mat.M32, mat.M33, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );
        }

        public static JMatrix ToJMatrix(Matrix matrix)
        {
            var jMatrix = new JMatrix();
            jMatrix.M11 = matrix.M11;
            jMatrix.M12 = matrix.M12;
            jMatrix.M13 = matrix.M13;
            jMatrix.M21 = matrix.M21;
            jMatrix.M22 = matrix.M22;
            jMatrix.M23 = matrix.M23;
            jMatrix.M31 = matrix.M31;
            jMatrix.M32 = matrix.M32;
            jMatrix.M33 = matrix.M33;
            return jMatrix;
        }
    }
}
