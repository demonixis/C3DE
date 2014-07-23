using Microsoft.Xna.Framework;

namespace C3DE
{
    public class Light : SceneObject
    {
        internal Matrix viewMatrix;
        internal Matrix projectionMatrix;
        internal Vector3 radius;
        internal Vector3 direction;

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        public Matrix WorldViewProjection
        {
            get { return Matrix.Identity * viewMatrix * projectionMatrix; }
        }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public float Radius
        {
            get { return radius.X; }
            set
            {
                radius.X = value;
                radius.Y = value;
                radius.Z = value;
            }
        }

        public Light()
            : base()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 500);
            Transform.Translate(6.0f, 30.0f, -23.0f);
            radius = new Vector3(250.0f);
            direction = new Vector3(50.0f, 330.0f, 0.0f);
        }

        public void Update(ref BoundingSphere sphere)
        {
            Vector3 dir = sphere.Center - transform.Position;
            dir.Normalize();

            viewMatrix = Matrix.CreateLookAt(transform.Position, sphere.Center, Vector3.Up);
            float size = sphere.Radius;

            float dist = Vector3.Distance(Transform.Position, sphere.Center);
            projectionMatrix = Matrix.CreateOrthographicOffCenter(-size, size, size, -size, dist - sphere.Radius, dist + sphere.Radius * 2);
        }
    }
}
