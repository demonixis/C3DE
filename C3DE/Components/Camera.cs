using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Runtime.Serialization;

namespace C3DE.Components
{
    public enum CameraProjectionType
    {
        Perspective = 0, Orthographic
    }

    [DataContract]
    public class Camera : Component
    {
        public static Camera Main { get; internal set; }
        protected internal Matrix m_ViewMatrix;
        protected internal Matrix m_ProjectionMatrix;
        protected internal Color clearColor;
        protected internal short depth;
        private Vector3 _target;
        private Vector3 _upVector;
        private float _fieldOfView;
        private float _aspectRatio;
        private float _nearPlane;
        private float _farPlane;
        private CameraProjectionType _projectionType;
        private bool _needUpdate;
        private bool _needProjectionUpdate;

        [DataMember]
        public float AspectRatio
        {
            get { return _aspectRatio; }
            set
            {
                if (_aspectRatio != value)
                {
                    _aspectRatio = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        [DataMember]
        public Color ClearColor
        {
            get { return clearColor; }
            set { clearColor = value; }
        }

        [DataMember]
        public short Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        [DataMember]
        public float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                if (_fieldOfView != value)
                {
                    _fieldOfView = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        [DataMember]
        public float Near
        {
            get { return _nearPlane; }
            set
            {
                if (_nearPlane != value)
                {
                    _nearPlane = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        [DataMember]
        public float Far
        {
            get { return _farPlane; }
            set
            {
                if (_farPlane != value)
                {
                    _farPlane = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        [DataMember]
        public Vector3 Target
        {
            get { return _target; }
            set { _target = value; }
        }

        [DataMember]
        public CameraProjectionType ProjectionType
        {
            get { return _projectionType; }
            set
            {
                if (value != _projectionType)
                {
                    _projectionType = value;
                    _needProjectionUpdate = true;
                }
            }
        }

        public Matrix ViewMatrix
        {
            get { return m_ViewMatrix; }
        }

        public Matrix ProjectionMatrix
        {
            get { return m_ProjectionMatrix; }
        }

        public Vector3 Forward => m_ViewMatrix.Forward;
        public Vector3 BackWard => m_ViewMatrix.Backward;
        public Vector3 Left => m_ViewMatrix.Left;
        public Vector3 Right => m_ViewMatrix.Right;

        public Vector3 Rotation
        {
            get
            {
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                m_ViewMatrix.Decompose(out scale, out rotation, out translation);
                return rotation.ToEuler();
            }
        }

        public Camera()
            : base()
        {
            _fieldOfView = MathHelper.ToRadians(45);
            _aspectRatio = (float)Application.GraphicsDevice.Viewport.Width / (float)Application.GraphicsDevice.Viewport.Height;
            _nearPlane = 1.0f;
            _farPlane = 500.0f;
            _projectionType = CameraProjectionType.Perspective;
            clearColor = Color.Black;
            depth = 0;
        }

        public override void Start()
        {
            Setup(m_Transform.LocalPosition, Vector3.Zero, Vector3.Up);
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
            m_Transform.LocalPosition = position;
            _target = camTarget;
            _upVector = upVector;

            m_ViewMatrix = Matrix.CreateLookAt(position, _target, Vector3.Up);

            ComputeProjectionMatrix();
        }

        public void ComputeProjectionMatrix()
        {
            if (_projectionType == CameraProjectionType.Perspective)
                m_ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlane, _farPlane);
            else
                m_ProjectionMatrix = Matrix.CreateOrthographic(Application.GraphicsDevice.Viewport.Width, Application.GraphicsDevice.Viewport.Height, _nearPlane, _farPlane);

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
            BoundingFrustum frustrum = new BoundingFrustum(m_ViewMatrix * m_ProjectionMatrix);
            return frustrum.GetCorners();
        }

        public void LookAt(Vector3 target)
        {
            var position = m_Transform.Position;
            m_ViewMatrix = Matrix.CreateLookAt(position, target, _upVector);
        }

        public override void Update()
        {
            if (_needProjectionUpdate)
                ComputeProjectionMatrix();

            if (!m_GameObject.IsStatic || _needUpdate)
            {
                var position = m_Transform.Position;
                var rotation = m_Transform.Rotation;
                var matrix = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                var target = position + Vector3.Transform(Vector3.Forward, matrix);

                m_ViewMatrix = Matrix.CreateLookAt(position, target, _upVector);
                _needUpdate = false;
            }
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            return Application.GraphicsDevice.Viewport.Project(position, m_ProjectionMatrix, m_ViewMatrix, Matrix.Identity);
        }

        /// <summary>
        /// Get 3D world position of from the 2D position (on screen)
        /// </summary>
        /// <param name="x">Position on world</param>
        /// <param name="x">Position on world</param>
        /// <returns>Position on 3D world</returns>
        public Vector3 ScreenToWorldPoint(float x, float y)
        {
            var device = Application.GraphicsDevice;
            var nearScreenPoint = new Vector3(x, y, 0.0f);
            var farScreenPoint = new Vector3(x, y, 1.0f);
            var nearWorldPoint = device.Viewport.Unproject(nearScreenPoint, m_ProjectionMatrix, m_ViewMatrix, Matrix.Identity);
            var farWorldPoint = device.Viewport.Unproject(farScreenPoint, m_ProjectionMatrix, m_ViewMatrix, Matrix.Identity);
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
            Vector3 nearPoint = new Vector3(position, 0);
            Vector3 farPoint = new Vector3(position, 1);

            nearPoint = Application.GraphicsDevice.Viewport.Unproject(nearPoint, m_ProjectionMatrix, m_ViewMatrix, Matrix.Identity);
            farPoint = Application.GraphicsDevice.Viewport.Unproject(farPoint, m_ProjectionMatrix, m_ViewMatrix, Matrix.Identity);

            // Get the direction
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public override int CompareTo(object obj)
        {
            var camera = obj as Camera;

            if (camera == null)
                return 1;

            if (depth == camera.depth)
                return 0;
            else if (depth > camera.depth)
                return 1;
            else
                return -1;
        }
    }
}
