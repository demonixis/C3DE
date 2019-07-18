using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components
{
    public enum CameraProjectionType
    {
        Perspective = 0, Orthographic
    }

    public class Camera : Component
    {
        public static Camera Main { get; internal set; }
        protected internal Matrix _viewMatrix;
        protected internal Matrix _projectionMatrix;
        protected internal Color _clearColor;
        protected internal BoundingFrustum _boundingFrustrum;
        protected internal short _depth;
        private Vector3 _target;
        private Vector3 _upVector;
        private float _fieldOfView;
        private float _aspectRatio;
        private float _nearPlane;
        private float _farPlane;
        private CameraProjectionType _projectionType;
        private bool _needUpdate;
        private bool _needProjectionUpdate;

        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                if (_aspectRatio != value)
                {
                    _aspectRatio = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        public Color ClearColor
        {
            get => _clearColor;
            set => _clearColor = value;
        }

        public short Depth
        {
            get => _depth;
            set => _depth = value;
        }

        public float FieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (_fieldOfView != value)
                {
                    _fieldOfView = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        public float Near
        {
            get => _nearPlane;
            set
            {
                if (_nearPlane != value)
                {
                    _nearPlane = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        public float Far
        {
            get => _farPlane;
            set
            {
                if (_farPlane != value)
                {
                    _farPlane = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        public Vector3 Target
        {
            get => _target;
            set => _target = value;
        }

        public CameraProjectionType ProjectionType
        {
            get => _projectionType;
            set
            {
                if (value != _projectionType)
                {
                    _projectionType = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        public RenderTarget2D RenderTarget { get; set; }

        public Matrix ViewMatrix => _viewMatrix; 

        public Matrix ProjectionMatrix =>  _projectionMatrix; 

        public Vector3 Forward => _viewMatrix.Forward;
        public Vector3 BackWard => _viewMatrix.Backward;
        public Vector3 Left => _viewMatrix.Left;
        public Vector3 Right => _viewMatrix.Right;

        public Vector3 Rotation
        {
            get
            {
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                _viewMatrix.Decompose(out scale, out rotation, out translation);
                return rotation.ToEuler();
            }
        }

        public Camera()
            : base()
        {
            _fieldOfView = MathHelper.ToRadians(45);
            _aspectRatio = (float)Application.GraphicsDevice.Viewport.Width / (float)Application.GraphicsDevice.Viewport.Height;
            _nearPlane = 0.5f;
            _farPlane = 500.0f;
            _projectionType = CameraProjectionType.Perspective;
            _clearColor = Color.Black;
            _depth = 0;
        }

        public override void Start()
        {
            Setup(_transform.LocalPosition, Vector3.Zero, Vector3.Up);
            Screen.ScreenSizeChanged += OnScreenChanged;
        }

        public override void Dispose()
        {
            base.Dispose();
            Screen.ScreenSizeChanged -= OnScreenChanged;
        }

        private void OnScreenChanged(int width, int height)
        {
            _aspectRatio = (float)width / (float)height;
            _needProjectionUpdate = true;
        }

        public void Setup(Vector3 position, Vector3 camTarget, Vector3 upVector)
        {
            _target = camTarget;
            _upVector = upVector;
            _viewMatrix = Matrix.CreateLookAt(position, _target, Vector3.Up);

            ComputeProjectionMatrix();

            _boundingFrustrum = new BoundingFrustum(_viewMatrix * _projectionMatrix);
        }

        public void ComputeProjectionMatrix()
        {
            if (_projectionType == CameraProjectionType.Perspective)
                _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlane, _farPlane);
            else
                _projectionMatrix = Matrix.CreateOrthographic(Application.GraphicsDevice.Viewport.Width, Application.GraphicsDevice.Viewport.Height, _nearPlane, _farPlane);

            _needProjectionUpdate = false;
        }

        public void ComputeProjectionMatrix(float fov, float aspect, float near, float far)
        {
            _fieldOfView = fov;
            _aspectRatio = aspect;
            _nearPlane = near;
            _farPlane = far;

            ComputeProjectionMatrix();
        }

        public Vector3[] CalculateFrustumCorners(Rectangle viewport, float z, int eye)
        {
            _needProjectionUpdate = true;
            _needUpdate = true;

            Update();

            var frustrum = new BoundingFrustum(_viewMatrix * _projectionMatrix);
            return frustrum.GetCorners();
        }

        public void LookAt(Vector3 target)
        {
            var position = _transform.Position;
            _viewMatrix = Matrix.CreateLookAt(position, target, _upVector);
        }

        public override void Update()
        {
            if (_needProjectionUpdate)
                ComputeProjectionMatrix();

            if (!_gameObject.IsStatic || _needUpdate)
            {
                var position = _transform.Position;
                var rotation = _transform.Rotation;
                var matrix = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                var target = position + Vector3.Transform(Vector3.Forward, matrix);

                _viewMatrix = Matrix.CreateLookAt(position, target, _upVector);
                _boundingFrustrum.Matrix = _viewMatrix * _projectionMatrix;
                _needUpdate = false;
            }
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            return Application.GraphicsDevice.Viewport.Project(position, _projectionMatrix, _viewMatrix, Matrix.Identity);
        }

        /// <summary>
        /// Get 3D world position of from the 2D position (on screen)
        /// </summary>
        /// <param name="x">Position on world</param>
        /// <param name="x">Position on world</param>
        /// <returns>Position on 3D world</returns>
        public Vector3 ScreenToWorldPoint(float x, float y)
        {
            var viewport = Application.GraphicsDevice.Viewport;
            var nearScreenPoint = new Vector3(x, y, 0.0f);
            var farScreenPoint = new Vector3(x, y, 1.0f);
            var nearWorldPoint = viewport.Unproject(nearScreenPoint, _projectionMatrix, _viewMatrix, Matrix.Identity);
            var farWorldPoint = viewport.Unproject(farScreenPoint, _projectionMatrix, _viewMatrix, Matrix.Identity);
            var direction = farWorldPoint - nearWorldPoint;
            var zFactor = -nearWorldPoint.Y / direction.Y;
            return nearWorldPoint + direction * zFactor;
        }

        /// <summary>
        /// Get 3D world position of from the 2D position (on screen)
        /// </summary>
        /// <param name="position">Position on world</param>
        /// <returns>Position on 3D world</returns>
        public Vector3 ScreenToWorld(Vector3 position) => ScreenToWorldPoint(position.X, position.Y);

        public Ray GetRay(Vector2 position)
        {
            var viewport = Application.GraphicsDevice.Viewport;
            var nearPoint = new Vector3(position, 0);
            var farPoint = new Vector3(position, 1);

            nearPoint = viewport.Unproject(nearPoint, _projectionMatrix, _viewMatrix, Matrix.Identity);
            farPoint = viewport.Unproject(farPoint, _projectionMatrix, _viewMatrix, Matrix.Identity);

            // Get the direction
            var direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public override int CompareTo(object obj)
        {
            var camera = obj as Camera;

            if (camera == null)
                return 1;

            if (_depth == camera._depth)
                return 0;
            else if (_depth > camera._depth)
                return 1;
            else
                return -1;
        }
    }
}
