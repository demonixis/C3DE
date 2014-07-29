using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Components.Cameras
{
    public enum CameraProjectionType
    {
        Perspective = 0, Orthographic
    }

    public class Camera : Component
    {
        protected internal Matrix view;
        protected internal Matrix projection;
        protected Vector3 reference;
        protected Transform transform;
        private Vector3 _target;
        private Vector3 _upVector;
        private float _fieldOfView;
        private float _aspectRatio;
        private float _nearPlane;
        private float _farPlane;
        private CameraProjectionType _projectionType;
        private bool _needUpdate;

        public Vector3 Reference
        {
            get { return reference; }
        }

        public Vector3 Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public CameraProjectionType ProjectionType
        {
            get { return _projectionType; }
            set
            {
                if (value != _projectionType)
                {
                    _projectionType = value;
                    ComputeProjectionMatrix();
                    _needUpdate = true;
                }
            }
        }

        public Camera()
            : this(null)
        {
        }

        public Camera(SceneObject sceneObject)
            : base(sceneObject)
        {
            _fieldOfView = MathHelper.ToRadians(45);
            _aspectRatio = (float)App.GraphicsDevice.Viewport.Width / (float)App.GraphicsDevice.Viewport.Height;
            _nearPlane = 1.0f;
            _farPlane = 500.0f;
            _projectionType = CameraProjectionType.Perspective;
            reference = new Vector3(0.0f, 0.0f, 1.0f);
        }

        public override void LoadContent(ContentManager content)
        {
            transform = GetComponent<Transform>();
            Setup(transform.LocalPosition, Vector3.Zero, Vector3.Up);
        }

        public void Setup(Vector3 position, Vector3 camTarget, Vector3 upVector)
        {
            sceneObject.Transform.Position = position;
            _target = camTarget;
            _upVector = upVector;
            _needUpdate = true;

            view = Matrix.CreateLookAt(sceneObject.Transform.LocalPosition, _target, Vector3.Up);

            ComputeProjectionMatrix();
            _needUpdate = true;
        }

        protected void ComputeProjectionMatrix()
        {
            if (_projectionType == CameraProjectionType.Perspective)
                projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlane, _farPlane);
            else
                projection = Matrix.CreateOrthographic(App.GraphicsDevice.Viewport.Width, App.GraphicsDevice.Viewport.Height, _nearPlane, _farPlane);

        }

        public override void Update()
        {
            if (!sceneObject.IsStatic || _needUpdate)
            {
                view = Matrix.CreateLookAt(transform.Position, _target, _upVector);
                _needUpdate = false;
            }
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            return App.GraphicsDevice.Viewport.Project(position, projection, view, Matrix.Identity);
        }

        /// <summary>
        /// Get 3D world position of from the 2D position (on screen)
        /// </summary>
        /// <param name="camera">Camera to use</param>
        /// <param name="position">Position on world</param>
        /// <returns>Position on 3D world</returns>
        public Vector3 ScreenToWorld(Vector3 position)
        {
            return App.GraphicsDevice.Viewport.Unproject(position, projection, view, Matrix.Identity);
        }

        public Ray GetRay(Vector2 position)
        {
            Vector3 nearPoint = new Vector3(position, 0);
            Vector3 farPoint = new Vector3(position, 1);

            nearPoint = App.GraphicsDevice.Viewport.Unproject(nearPoint, projection, view, Matrix.Identity);
            farPoint = App.GraphicsDevice.Viewport.Unproject(farPoint, projection, view, Matrix.Identity);

            // Get the direction
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }
    }
}
