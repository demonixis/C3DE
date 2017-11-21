using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.Serialization;

namespace C3DE.Components.Controllers
{
    /// <summary>
    /// An orbit controller component.
    /// It allows user to move and rotate the camera around a point.
    /// </summary>
    [DataContract]
    public class OrbitController : Controller
    {
        private Camera _camera;
        private float _distance;
        private Vector2 _angle;
        private Vector3 _position;
        private Vector3 _target;
        private Vector3 _cacheVec3;
        protected Vector2 angleVelocity;
        protected Vector3 positionVelicoty;
        protected float distanceVelocity;
        
        /// <summary>
        /// Gets or sets the min angle on y-axis.
        /// </summary>
        [DataMember]
        public float MinAngle { get; set; }

        /// <summary>
        /// Gets or sets the max angle on y-axis.
        /// </summary>
        [DataMember]
        public float MaxAngle { get; set; }

        /// <summary>
        /// Gets or sets the min distance from the target.
        /// </summary>
        [DataMember]
        public float MinDistance { get; set; }

        /// <summary>
        /// Gets or sets the max distance from the target.
        /// </summary>
        [DataMember]
        public float MaxDistance { get; set; }

        [DataMember]
        public float Distance
        {
            get { return _distance; }
            set { _distance = Math.Max(Math.Min(value, MaxDistance), MinDistance); }
        }

        /// <summary>
        /// Create an orbit controller with default values.
        /// </summary>
        public OrbitController()
            : base()
        {
            _angle = new Vector2(0.0f, MathHelper.Pi / 6.0f);
            _distance = 35;

            MinAngle = -MathHelper.PiOver2 + 0.1f;
            MaxAngle = MathHelper.PiOver2 - 0.1f;
            MinDistance = 1.0f;
            MaxDistance = 100.0f;
            RotationSpeed = 0.05f;
            MoveSpeed = 2.0f;
            StrafeSpeed = 1.75f;
            GamepadSensibility = new Vector2(2.5f);
            MouseSensibility = new Vector2(1.0f);
            Velocity = 0.95f;
            AngularVelocity = 0.95f;
			TouchSensibility = 0.45f;
            KeyboardEnabled = false;
        }

        public override void Start()
        {
            _camera = GetComponent<Camera>();

            if (_camera == null)
                throw new Exception("No camera attached to this scene object.");
        }

        public override void Update()
        {
            UpdateInputs();

            _angle += angleVelocity;
            _distance += distanceVelocity;
            _target += positionVelicoty;

            CheckAngle();
            CheckDistance();

            _position = Vector3.Transform(Vector3.Forward, Matrix.CreateFromYawPitchRoll(_angle.X, _angle.Y, 0));
            _position *= _distance;
            _position += _camera.Target;

            m_Transform.LocalPosition = _position;
            _camera.Target = _target;

            angleVelocity *= AngularVelocity;
            distanceVelocity *= Velocity;
            positionVelicoty *= Velocity;
        }

        public void LookAt(Transform transform)
        {
            _target = transform.LocalPosition;
        }

        protected override void UpdateInputs()
        {
            UpdateKeyboardInput();
            UpdateMouseInput();
            UpdateGamepadInput();
            UpdateTouchInput();
        }

        protected override void UpdateKeyboardInput()
        {
            if (!KeyboardEnabled)
                return;

            if (Input.Keys.Pressed(Keys.PageDown))
                distanceVelocity += MoveSpeed * Time.DeltaTime;
            else if (Input.Keys.Pressed(Keys.PageUp))
                distanceVelocity -= MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Up)
				angleVelocity.Y -= RotationSpeed * Time.DeltaTime * 25.0f;
            else if (Input.Keys.Down)
				angleVelocity.Y += RotationSpeed * Time.DeltaTime * 25.0f;

            if (Input.Keys.Left)
				angleVelocity.X -= RotationSpeed * Time.DeltaTime * 25.0f;
            else if (Input.Keys.Right)
				angleVelocity.X += RotationSpeed * Time.DeltaTime * 25.0f;
        }

        protected override void UpdateMouseInput()
        {
            if (!MouseEnabled)
                return;

            if (Input.Mouse.Down(Inputs.MouseButton.Left) && Input.Mouse.Drag())
            {
                angleVelocity.X -= RotationSpeed * Input.Mouse.Delta.X * MouseSensibility.X * Time.DeltaTime;
                angleVelocity.Y += RotationSpeed * Input.Mouse.Delta.Y * MouseSensibility.Y * Time.DeltaTime;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Right))
            {
                positionVelicoty.X += StrafeSpeed * Input.Mouse.Delta.X * MouseSensibility.X * Time.DeltaTime;
                positionVelicoty.Y += StrafeSpeed * Input.Mouse.Delta.Y * MouseSensibility.Y * Time.DeltaTime;
            }

            distanceVelocity -= Input.Mouse.Wheel * 0.05f * MoveSpeed * Time.DeltaTime;
        }

        protected override void UpdateGamepadInput()
        {
            if (!GamepadEnabled)
                return;

            angleVelocity += Input.Gamepad.LeftStickValue() * RotationSpeed * Time.DeltaTime * GamepadSensibility * 25.0f;

            positionVelicoty.X += Input.Gamepad.RightStickValue().X * StrafeSpeed * Time.DeltaTime * GamepadSensibility.X;
            positionVelicoty.Y += Input.Gamepad.RightStickValue().Y * StrafeSpeed * Time.DeltaTime * GamepadSensibility.Y;

            if (Input.Gamepad.LeftShoulder())
                distanceVelocity += MoveSpeed * Time.DeltaTime * GamepadSensibility.X;
            else if (Input.Gamepad.RightShoulder())
                distanceVelocity -= MoveSpeed * Time.DeltaTime * GamepadSensibility.X;
        }

		protected override void UpdateTouchInput()
        {
            if (!TouchEnabled)
                return;

            if (Input.Touch.TouchCount == 1)
				angleVelocity -= Input.Touch.Delta () * RotationSpeed * Time.DeltaTime * TouchSensibility;
			else if (Input.Touch.TouchCount == 2)
				distanceVelocity += Input.Touch.Delta ().X * MoveSpeed * Time.DeltaTime;
            else if (Input.Touch.TouchCount == 3)
            {
                _cacheVec3.X = Input.Touch.Delta().X;
                _cacheVec3.Y = Input.Touch.Delta().Y;
                _cacheVec3.Z = 0;
                positionVelicoty += _cacheVec3 * StrafeSpeed * Time.DeltaTime * 0.5f;
            }
        }

        private void CheckAngle()
        {
            if (_angle.Y > MaxAngle)
                _angle.Y = MaxAngle;
            else if (_angle.Y < MinAngle)
                _angle.Y = MinAngle;
        }

        private void CheckDistance()
        {
            if (_distance > MaxDistance)
                _distance = MaxDistance;
            else if (_distance < MinDistance)
                _distance = MinDistance;
        }
    }
}
