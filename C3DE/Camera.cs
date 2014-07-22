using Microsoft.Xna.Framework;

namespace C3DE
{
    public class Camera
    {
        internal Vector3 position;
        internal Vector3 rotation;
        internal Vector3 target;
        internal Matrix view;
        internal Matrix projection;
        private Vector3 reference;
        private Vector3 upVector;
        protected float fieldOfView;
        protected float aspectRatio;
        protected float nearClip;
        protected float farClip;

        public Camera(int width, int height)
        {
            fieldOfView = MathHelper.ToRadians(45);
            aspectRatio = (float)width / (float)height;
            nearClip = 1.0f;
            farClip = 500.0f;
            reference = new Vector3(0.0f, 0.0f, 1.0f);

            position = new Vector3(0, 0.5f, -10);
            target = Vector3.Zero;
            view = Matrix.CreateLookAt(position, target, Vector3.Up);

            ComputePerspective();
            UpdateTarget();
        }

        public void Update()
        {
            view = Matrix.CreateLookAt(position, target, Vector3.Up);
        }

        public void ComputePerspective()
        {
            projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClip, farClip);
        }

        public virtual void Translate(ref Vector3 move)
        {
            Matrix forwardMovement = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            Vector3 v = Vector3.Transform(move, forwardMovement);

            position.X += v.X;
            position.Y += v.Y;
            position.Z += v.Z;

            UpdateTarget();
        }

        public virtual void Translate(float x, float y, float z)
        {
            var move = new Vector3(x, y, z);
            Translate(ref move);
        }

        public virtual void Rotate(ref Vector3 rot)
        {
            rotation.X += rot.X;
            rotation.Y += rot.Y;
            rotation.Z += rot.Z;

            UpdateTarget();
        }

        public virtual void Rotate(float rx, float ry, float rz)
        {
            rotation.X += rx;
            rotation.Y += ry;
            rotation.Z += rz;

            UpdateTarget();
        }

        private void UpdateTarget()
        {
            Matrix matRotation = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            Vector3 transformedReference = Vector3.Transform(reference, matRotation);
            target = position + transformedReference;
        }
    }
}
