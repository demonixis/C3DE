using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using Jitter.Collision.Shapes;
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
            get => m_rigidBody?.IsStatic ?? false;
            set
            {
                if (m_rigidBody != null)
                    m_rigidBody.IsStatic = value;
            }
        }

        public bool AffectedByGravity
        {
            get => m_rigidBody?.AffectedByGravity ?? false;
            set
            {
                if (m_rigidBody != null)
                    m_rigidBody.AffectedByGravity = value;
            }
        }

        public Rigidbody()
            : base()
        {
            var shape = new BoxShape(1, 1, 1);
            m_rigidBody = new JRigidBody(shape);
        }

        public override void Start()
        {
            base.Start();

            var renderer = GameObject.GetComponentInChildren<Renderer>();
            var collider = GameObject.GetComponentInChildren<Collider>();

            if (collider != null)
                SetShapeSource(collider);
            else if (renderer != null)
                SetShapeSource(renderer);

            SyncTransform();
        }

        public void SyncTransform()
        {
            m_rigidBody.Position = ToJVector(transform.Position);
            m_rigidBody.Orientation = ToJMatrix(Matrix.CreateFromYawPitchRoll(transform.Rotation.Y, transform.Rotation.Y, transform.Rotation.Z));
        }

        public void SetShapeSource(Collider collider)
        {
            var shape = new BoxShape(MathHelper.Max(collider.Size.X, 0.1f), MathHelper.Max(collider.Size.Y, 0.1f), MathHelper.Max(collider.Size.Z, 0.1f));
            m_rigidBody.Shape = shape;

            if (!m_AddedToScene && Scene.current != null)
            {
                Scene.current.m_PhysicsWorld.AddBody(m_rigidBody);
                m_AddedToScene = true;
            }
        }

        public void SetShapeSource(Renderer renderer)
        {
            var shape = new SphereShape(renderer.boundingSphere.Radius);
            m_rigidBody.Shape = shape;

            if (!m_AddedToScene && Scene.current != null)
            {
                Scene.current.m_PhysicsWorld.AddBody(m_rigidBody);
                m_AddedToScene = true;
            }
        }

        public override void Update()
        {
            base.Update();

            if (!m_AddedToScene)
                return;

            transform.Position = ToVector3(m_rigidBody.Position);
        }

        public override void Dispose()
        {
            if (m_rigidBody != null)
                Scene.current?.m_PhysicsWorld.RemoveBody(m_rigidBody);
        }

        public static Vector3 ToVector3(JVector jVector) => new Vector3(jVector.X, jVector.Y, jVector.Z);
        public static JVector ToJVector(Vector3 vector) => new JVector(vector.X, vector.Y, vector.Z);

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
