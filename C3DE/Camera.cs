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
        private Vector3 _reference;
        private float _fieldOfView;
        private float _aspectRatio;
        private float _nearClip;
        private float _farClip;
        private bool _dynamic;

        public bool Dynamic
        {
            get { return _dynamic; }
            set { _dynamic = value; }
        }

        public Camera(int width, int height)
        {
            _fieldOfView = MathHelper.ToRadians(45);
            _aspectRatio = (float)width / (float)height;
            _nearClip = 1.0f;
            _farClip = 500.0f;
            _reference = new Vector3(0.0f, 0.0f, 1.0f);
            _dynamic = true;

            position = new Vector3(0, 0.5f, -10);
            target = Vector3.Zero;
            view = Matrix.CreateLookAt(position, target, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearClip, _farClip);
            
            UpdateTarget();
        }

        public void Update()
        {
            if (_dynamic)
                view = Matrix.CreateLookAt(position, target, Vector3.Up);
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
            Vector3 transformedReference = Vector3.Transform(_reference, matRotation);
            target = position + transformedReference;
        }
    }
}
