using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        protected internal Color m_ClearColor;
        protected internal BoundingFrustum m_BoundingFrustrum;
        protected internal short m_Depth;
        private Vector3 m_Target;
        private Vector3 m_UpVector;
        private float m_FieldOfView;
        private float m_AspectRatio;
        private float m_NearPlane;
        private float m_FarPlane;
        private CameraProjectionType m_ProjectionType;
        private bool m_NeedUpdate;
        private bool m_NeedProjectionUpdate;

        [DataMember]
        public float AspectRatio
        {
            get { return m_AspectRatio; }
            set
            {
                if (m_AspectRatio != value)
                {
                    m_AspectRatio = value;
                    m_NeedProjectionUpdate = true;
                }
            }
        }

        [DataMember]
        public Color ClearColor
        {
            get { return m_ClearColor; }
            set { m_ClearColor = value; }
        }

        [DataMember]
        public short Depth
        {
            get { return m_Depth; }
            set { m_Depth = value; }
        }

        [DataMember]
        public float FieldOfView
        {
            get { return m_FieldOfView; }
            set
            {
                if (m_FieldOfView != value)
                {
                    m_FieldOfView = value;
                    m_NeedProjectionUpdate = true;
                }
            }
        }

        [DataMember]
        public float Near
        {
            get { return m_NearPlane; }
            set
            {
                if (m_NearPlane != value)
                {
                    m_NearPlane = value;
                    m_NeedProjectionUpdate = true;
                }
            }
        }

        [DataMember]
        public float Far
        {
            get { return m_FarPlane; }
            set
            {
                if (m_FarPlane != value)
                {
                    m_FarPlane = value;
                    m_NeedProjectionUpdate = true;
                }
            }
        }

        [DataMember]
        public Vector3 Target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        [DataMember]
        public CameraProjectionType ProjectionType
        {
            get { return m_ProjectionType; }
            set
            {
                if (value != m_ProjectionType)
                {
                    m_ProjectionType = value;
                    m_NeedProjectionUpdate = true;
                }
            }
        }

        public RenderTarget2D RenderTarget
        {
            get; set;
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
            m_FieldOfView = MathHelper.ToRadians(45);
            m_AspectRatio = (float)Application.GraphicsDevice.Viewport.Width / (float)Application.GraphicsDevice.Viewport.Height;
            m_NearPlane = 1.0f;
            m_FarPlane = 500.0f;
            m_ProjectionType = CameraProjectionType.Perspective;
            m_ClearColor = Color.Black;
            m_Depth = 0;
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
            m_AspectRatio = (float)width / (float)height;
            m_NeedProjectionUpdate = true;
        }

        public void Setup(Vector3 position, Vector3 camTarget, Vector3 upVector)
        {
            m_Target = camTarget;
            m_UpVector = upVector;

            m_ViewMatrix = Matrix.CreateLookAt(position, m_Target, Vector3.Up);

            ComputeProjectionMatrix();

            m_BoundingFrustrum = new BoundingFrustum(m_ViewMatrix * m_ProjectionMatrix);
        }

        public void ComputeProjectionMatrix()
        {
            if (m_ProjectionType == CameraProjectionType.Perspective)
                m_ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(m_FieldOfView, m_AspectRatio, m_NearPlane, m_FarPlane);
            else
                m_ProjectionMatrix = Matrix.CreateOrthographic(Application.GraphicsDevice.Viewport.Width, Application.GraphicsDevice.Viewport.Height, m_NearPlane, m_FarPlane);

            m_NeedProjectionUpdate = false;
        }

        public void ComputeProjectionMatrix(float fov, float aspect, float near, float far)
        {
            m_FieldOfView = fov;
            m_AspectRatio = aspect;
            m_NearPlane = near;
            m_FarPlane = far;

            ComputeProjectionMatrix();
        }

        public Vector3[] CalculateFrustumCorners(Rectangle viewport, float z, int eye)
        {
            m_NeedProjectionUpdate = true;
            m_NeedUpdate = true;
            Update();
            BoundingFrustum frustrum = new BoundingFrustum(m_ViewMatrix * m_ProjectionMatrix);
            return frustrum.GetCorners();
        }

        public void LookAt(Vector3 target)
        {
            var position = m_Transform.Position;
            m_ViewMatrix = Matrix.CreateLookAt(position, target, m_UpVector);
        }

        public override void Update()
        {
            if (m_NeedProjectionUpdate)
                ComputeProjectionMatrix();

            if (!m_GameObject.IsStatic || m_NeedUpdate)
            {
                var position = m_Transform.Position;
                var rotation = m_Transform.Rotation;
                var matrix = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                var target = position + Vector3.Transform(Vector3.Forward, matrix);

                m_ViewMatrix = Matrix.CreateLookAt(position, target, m_UpVector);
                m_BoundingFrustrum.Matrix = m_ViewMatrix * m_ProjectionMatrix;
                m_NeedUpdate = false;
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

            if (m_Depth == camera.m_Depth)
                return 0;
            else if (m_Depth > camera.m_Depth)
                return 1;
            else
                return -1;
        }
    }
}
