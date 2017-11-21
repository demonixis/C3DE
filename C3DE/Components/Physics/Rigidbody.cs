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
        private JRigidBody m_rigidBody;
        private bool m_AddedToScene;

        public bool IsStatic
        {
            get => m_rigidBody.IsStatic;
            set => m_rigidBody.IsStatic = value;
        }

        public bool AffectedByGravity
        {
            get => m_rigidBody?.AffectedByGravity ?? false;
            set => m_rigidBody.AffectedByGravity = value;
        }

        public Vector3 AngularVelocity
        {
            get => ToVector3(m_rigidBody.AngularVelocity);
            set => m_rigidBody.AngularVelocity = ToJVector(value);
        }

        public Vector3 Velocity
        {
            get => ToVector3(m_rigidBody.LinearVelocity);
            set => m_rigidBody.LinearVelocity = ToJVector(value);
        }

        public Material PhysicsMaterial
        {
            get => m_rigidBody.Material;
            set => m_rigidBody.Material = value;
        }

        public float Mass
        {
            get => m_rigidBody.Mass;
            set => m_rigidBody.Mass = value;
        }

        public bool IsKinematic { get; set; } = false;

        public JRigidBody JitterRigidbody
        {
            get => m_rigidBody;
        }

        public Shape Shape => m_rigidBody.Shape;

        public Rigidbody()
            : base()
        {
            var shape = new BoxShape(1.0f, 1.0f, 1.0f);
            m_rigidBody = new JRigidBody(shape);
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
            m_rigidBody.Position = ToJVector(transform.LocalPosition);
            m_rigidBody.Orientation = ToJMatrix(Matrix.CreateFromYawPitchRoll(transform.LocalRotation.Y, transform.LocalRotation.Y, transform.LocalRotation.Z));
        }

        public void SetShape(Shape shape)
        {
            m_rigidBody.Shape = shape;

            SyncTransform();
            shape.UpdateShape();

            if (!m_AddedToScene && Scene.current != null)
            {
                Scene.current.m_PhysicsWorld.AddBody(m_rigidBody);
                m_AddedToScene = true;
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
            m_rigidBody.AddForce(ToJVector(force));
        }

        public void AddForceAtPosition(Vector3 force, Vector3 position)
        {
            m_rigidBody.AddForce(ToJVector(force), ToJVector(position));
        }

        public void AddTorque(Vector3 torque)
        {
            m_rigidBody.AddTorque(ToJVector(torque));
        }

        public override void Update()
        {
            base.Update();

            if (!m_AddedToScene)
                return;

            if (!IsKinematic)
            {
                transform.SetPosition(ToVector3(m_rigidBody.Position));
                transform.SetRotation(ToMatrix(m_rigidBody.Orientation));
            }
            else
                SyncTransform();
        }

        public override void Dispose()
        {
            if (m_rigidBody != null)
                Scene.current?.m_PhysicsWorld.RemoveBody(m_rigidBody);
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
