using Microsoft.Xna.Framework;

namespace C3DE
{
    public class Light : SceneObject
    {
        internal Matrix viewMatrix;
        internal Matrix projectionMatrix;
        internal Vector3 radius;

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

        public Light()
            : base()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 500);
            Transform.Translate(2, 10, 2);
            radius = new Vector3(250.0f);
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
