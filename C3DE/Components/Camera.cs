using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Components
{
    public enum CameraProjectionType
    {
        Perspective = 0, Orthographic
    }

    public class Camera : Component
    {
        public static Camera main { get; internal set; }
        protected internal Matrix view;
        protected internal Matrix projection;
        protected internal Color clearColor;
		protected internal short depth;
        protected Vector3 reference;
        private Vector3 _target;
        private Vector3 _upVector;
        private float _fieldOfView;
        private float _aspectRatio;
        private float _nearPlane;
        private float _farPlane;
        private CameraProjectionType _projectionType;
        private bool _needUpdate;
        private bool _needProjectionUpdate;

        public float Aspect
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

        public Color ClearColor
        {
            get { return clearColor; }
            set { clearColor = value; }
        }

		public short Depth
		{
			get { return depth; }
			set { depth = value; }
		}

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
                    _needProjectionUpdate = true;
                }
            }
        }

        public Matrix ViewMatrix
        {
            get { return view; }
        }

        public Matrix ProjectionMatrix
        {
            get { return projection; }
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
            reference = new Vector3(0.0f, 0.0f, 1.0f);
        }

        public override void Start()
        {
            Setup(transform.Position, Vector3.Zero, Vector3.Up);
        }

        public void Setup(Vector3 position, Vector3 camTarget, Vector3 upVector)
        {
            transform.Position = position;
            _target = camTarget;
            _upVector = upVector;

            view = Matrix.CreateLookAt(position, _target, Vector3.Up);

            ComputeProjectionMatrix();
        }

        public void ComputeProjectionMatrix()
        {
            if (_projectionType == CameraProjectionType.Perspective)
                projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlane, _farPlane);
            else
                projection = Matrix.CreateOrthographic(Application.GraphicsDevice.Viewport.Width, Application.GraphicsDevice.Viewport.Height, _nearPlane, _farPlane);

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

        public override void Update()
        {
            if (_needProjectionUpdate)
                ComputeProjectionMatrix();

            if (!sceneObject.IsStatic || _needUpdate)
            {
                view = Matrix.CreateLookAt(transform.Position, _target, _upVector);
                _needUpdate = false;
            }
        }

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
            return Application.GraphicsDevice.Viewport.Project(position, projection, view, Matrix.Identity);
        }

        /// <summary>
        /// Get 3D world position of from the 2D position (on screen)
        /// </summary>
        /// <param name="camera">Camera to use</param>
        /// <param name="position">Position on world</param>
        /// <returns>Position on 3D world</returns>
        public Vector3 ScreenToWorld(Vector3 position)
        {
            return Application.GraphicsDevice.Viewport.Unproject(position, projection, view, Matrix.Identity);
        }

        public Ray GetRay(Vector2 position)
        {
            Vector3 nearPoint = new Vector3(position, 0);
            Vector3 farPoint = new Vector3(position, 1);

            nearPoint = Application.GraphicsDevice.Viewport.Unproject(nearPoint, projection, view, Matrix.Identity);
            farPoint = Application.GraphicsDevice.Viewport.Unproject(farPoint, projection, view, Matrix.Identity);

            // Get the direction
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public override SerializedCollection Serialize()
        {
            var data = base.Serialize();
            data.IncreaseCapacity(10);
            data.Add("Color", SerializerHelper.ToString(clearColor));
            data.Add("Depth", depth.ToString());
            data.Add("Reference", SerializerHelper.ToString(reference));
            data.Add("Target", SerializerHelper.ToString(_target));
            data.Add("Up", SerializerHelper.ToString(_upVector));
            data.Add("FOV", _fieldOfView.ToString());
            data.Add("Aspect", _aspectRatio.ToString());
            data.Add("Near", _nearPlane.ToString());
            data.Add("Far", _farPlane.ToString());
            data.Add("Type", ((int)_projectionType).ToString());

            return data;
        }

        public override void Deserialize(SerializedCollection data)
        {
            base.Deserialize(data);

            clearColor = SerializerHelper.ToColor(data["Color"]);
            depth = short.Parse(data["Depth"]);
            reference = SerializerHelper.ToVector3(data["Reference"]);
            _target = SerializerHelper.ToVector3(data["Target"]);
            _upVector = SerializerHelper.ToVector3(data["Up"]);
            _fieldOfView = float.Parse(data["FOV"]);
            _aspectRatio = float.Parse(data["Aspect"]);
            _nearPlane = float.Parse(data["Near"]);
            _farPlane = float.Parse(data["Far"]);
            _projectionType = (CameraProjectionType)int.Parse(data["Type"]);
            _needProjectionUpdate = true;
            _needUpdate = true;
        }
    }
}
