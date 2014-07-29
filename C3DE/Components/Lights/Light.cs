using Microsoft.Xna.Framework;

namespace C3DE.Components.Lights
{
    public class Light : Component
    {
        internal Matrix viewMatrix;
        internal Matrix projectionMatrix;

        protected Color diffuseColor;
        protected Color specularColor;
        protected float intensity;
        protected Vector3 radius;

        public Matrix View
        {
            get { return viewMatrix; }
        }

        public Matrix Projection
        {
            get { return projectionMatrix; }
        }

        public Matrix ViewProjection
        {
            get { return viewMatrix * projectionMatrix; }
        }

        public Color DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }

        public Color SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; }
        }

        public float Intensity
        {
            get { return intensity; }
            set { intensity = value; }
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
            : this(null)
        {
        }

        public Light(SceneObject sceneObject)
            : base(sceneObject)
        {
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1, 1, 500);
            diffuseColor = Color.White;
            specularColor = Color.Black;
            intensity = 1.0f;
            radius = new Vector3(250.0f);
            //sceneObject.Transform.Translate(6.0f, 30.0f, -23.0f);
        }

        // Need to be changed quickly !
        public void Update(ref BoundingSphere sphere)
        {
            Vector3 dir = sphere.Center - sceneObject.Transform.Position;
            dir.Normalize();

            viewMatrix = Matrix.CreateLookAt(sceneObject.Transform.Position, sphere.Center, Vector3.Up);
            float size = sphere.Radius;

            float dist = Vector3.Distance(sceneObject.Transform.Position, sphere.Center);
            projectionMatrix = Matrix.CreateOrthographicOffCenter(-size, size, size, -size, dist - sphere.Radius, dist + sphere.Radius * 2);
        }
    }
}
