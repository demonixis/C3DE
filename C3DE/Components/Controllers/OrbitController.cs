using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Components.Controllers
{
    /// <summary>
    /// An orbit controller component to attach to a SceneObject with a Camera component.
    /// It allows user to move and rotate the camera around a point.
    /// </summary>
    public class OrbitController : Component
    {
        private Camera _camera;
        private Transform _transform;
        private float _distance;
        private Vector2 _angle;
        private Vector3 _position;
        private Vector3 _target;
        private Vector2 _angleVelocity;
        private Vector3 _positionVelicoty;
        private float _distanceVelocity;

        public float MinAngle { get; set; }
        public float MaxAngle { get; set; }
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public float RotationSpeed { get; set; }
        public float MoveSpeed { get; set; }
        public float StrafeSpeed { get; set; }
        public float GamepadSensibility { get; set; }
        public float Velocity { get; set; }
        public float AngularVelocity { get; set; }

        public OrbitController()
            : this(null)
        {
        }

        public OrbitController(SceneObject sceneObject)
            : base(sceneObject)
        {
            _angle = new Vector2(0.0f, -MathHelper.Pi / 6.0f);
            _distance = 35;

            MinAngle = -MathHelper.PiOver2 + 0.1f;
            MaxAngle = MathHelper.PiOver2 - 0.1f;
            MinDistance = 1.0f;
            MaxDistance = 100.0f;
            RotationSpeed = 0.05f;
            MoveSpeed = 2.0f;
            StrafeSpeed = 1.75f;
            GamepadSensibility = 2.5f;
            Velocity = 0.95f;
            AngularVelocity = 0.95f;
        }

        public override void LoadContent(ContentManager content)
        {
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();
        }

        public override void Update()
        {
            UpdateKeyboardInput();
            UpdateMouseInput();
            UpdateGamepadInput();

            _angle += _angleVelocity;
            _distance += _distanceVelocity;
            _target += _positionVelicoty;

            CheckAngle();
            CheckDistance();

            _position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(_angle.X, _angle.Y, 0));
            _position *= _distance;
            _position += _camera.Target;

            _transform.Position = _position;
            _camera.Target = _target;

            _angleVelocity *= AngularVelocity;
            _distanceVelocity *= Velocity;
            _positionVelicoty *= Velocity;
        }

        private void UpdateKeyboardInput()
        {
            if (Input.Keys.Pressed(Keys.PageDown))
                _distanceVelocity += MoveSpeed * Time.DeltaTime;
            else if (Input.Keys.Pressed(Keys.PageUp))
                _distanceVelocity -= MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Up)
                _angle.Y -= RotationSpeed * Time.DeltaTime * 25.0f;
            else if (Input.Keys.Down)
                _angle.Y += RotationSpeed * Time.DeltaTime * 25.0f;

            if (Input.Keys.Left)
                _angle.X -= RotationSpeed * Time.DeltaTime * 25.0f;
            else if (Input.Keys.Right)
                _angle.X += RotationSpeed * Time.DeltaTime * 25.0f;
        }

        private void UpdateMouseInput()
        {
            if (Input.Mouse.Down(Inputs.MouseButton.Left) && Input.Mouse.Drag())
            {
                _angleVelocity.X -= RotationSpeed * Input.Mouse.Delta.X * Time.DeltaTime;
                _angleVelocity.Y -= RotationSpeed * Input.Mouse.Delta.Y * Time.DeltaTime;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Right))
            {
                _positionVelicoty.X += StrafeSpeed * Input.Mouse.Delta.X * Time.DeltaTime;
                _positionVelicoty.Y += StrafeSpeed * Input.Mouse.Delta.Y * Time.DeltaTime;
            }

            _distanceVelocity -= Input.Mouse.Wheel / 4.0f * MoveSpeed * Time.DeltaTime;
        }

        private void UpdateGamepadInput()
        {
            _angle += Input.Gamepad.LeftStickValue() * RotationSpeed * Time.DeltaTime * GamepadSensibility * 25.0f;

            _positionVelicoty.X += Input.Gamepad.RightStickValue().X * StrafeSpeed * Time.DeltaTime * GamepadSensibility;
            _positionVelicoty.Y += Input.Gamepad.RightStickValue().Y * StrafeSpeed * Time.DeltaTime * GamepadSensibility;

            if (Input.Gamepad.LeftShoulder())
                _distanceVelocity += MoveSpeed * Time.DeltaTime * GamepadSensibility;
            else if (Input.Gamepad.RightShoulder())
                _distanceVelocity -= MoveSpeed * Time.DeltaTime * GamepadSensibility;
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
