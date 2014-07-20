using Microsoft.Xna.Framework;

namespace C3DE
{
    public class Light
    {
        internal Matrix viewMatrix;
        internal Matrix projectionMatrix;
        internal Vector3 position;
        internal Vector3 radius;

        public Vector3 Position
        {
            get { return position; }
        }

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
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 500);
            position = new Vector3(2, 10, 2);
            radius = new Vector3(250.0f);
        }

        public void Translate(float x, float y, float z)
        {
            position.X += x;
            position.Y += y;
            position.Z += z;
            viewMatrix = Matrix.CreateLookAt(position, new Vector3(-2, 3, -10), Vector3.Up);
        }

        public void Move(Vector3 pos)
        {
            position = pos;
            viewMatrix = Matrix.CreateLookAt(position, Vector3.Zero, Vector3.Up);
        }

        public void Update(ref BoundingSphere sphere)
        {
            Vector3 dir = sphere.Center - position;
            dir.Normalize();

            viewMatrix = Matrix.CreateLookAt(position, sphere.Center, Vector3.Up);
            float size = sphere.Radius;

            float dist = Vector3.Distance(position, sphere.Center);
            projectionMatrix = Matrix.CreateOrthographicOffCenter(-size, size, size, -size, dist - sphere.Radius, dist + sphere.Radius * 2);
        }
    }
}
