using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Components.Controllers
{
    public class FirstPersonController : Component
    {
        private Camera _camera;
        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;
        private Vector3 _translation = Vector3.Zero;
        private Vector3 _rotation = Vector3.Zero;
        public float Velocity { get; set; }
        public float AngularVelocity { get; set; }
        public float MoveSpeed { get; set; }
        public float RotationSpeed { get; set; }
        public float LookSpeed { get; set; }
        public float StrafeSpeed { get; set; }
        public bool Fly { get; set; }
        public Vector2 MouseSensibility { get; set; }

        public FirstPersonController()
            : base()
        {
            Velocity = 0.95f;
            AngularVelocity = 0.95f;
            MoveSpeed = 1.0f;
            RotationSpeed = 0.1f;
            LookSpeed = 0.15f;
            StrafeSpeed = 0.75f;
            MouseSensibility = new Vector2(0.15f, 0.15f);
            Fly = false;
        }

        public override void Start()
        {
            _camera = GetComponent<Camera>();

            if (_camera == null)
                throw new Exception("No camera attached to this scene object.");
        }

        public override void Update()
        {
            UpdateMouseInput();
            UpdateKeyboardInput();
            UpdateGamepadInput();

            _rotationMatrix = Matrix.CreateFromYawPitchRoll(transform.Rotation.Y, transform.Rotation.X, 0.0f);

            _transformedReference = Vector3.Transform(_translation, !Fly ? Matrix.CreateRotationY(transform.Rotation.Y) : _rotationMatrix);

            // Translate and rotate
            transform.Translate(ref _transformedReference);
            transform.Rotate(ref _rotation);

            // Update target
            _camera.Target = transform.Position + Vector3.Transform(_camera.Reference, _rotationMatrix);

            _translation *= Velocity;
            _rotation *= AngularVelocity;
        }

        private void UpdateKeyboardInput()
        {
            if (Input.Keys.Up || Input.Keys.Pressed(Keys.W))
                _translation.Z += MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down) || Input.Keys.Pressed(Keys.S))
                _translation.Z -= MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.A))
                _translation.X += MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.D))
                _translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            if (Input.Keys.Pressed(Keys.Q))
                _translation.Y += StrafeSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.E))
                _translation.Y -= StrafeSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.PageUp))
                _rotation.X -= LookSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.PageDown))
                _rotation.X += LookSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.Left))
                _rotation.Y += RotationSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Right))
                _rotation.Y -= RotationSpeed * Time.DeltaTime;

            if (Input.Keys.JustPressed(Keys.Tab))
                Fly = !Fly;
        }

        private void UpdateMouseInput()
        {
            if (Input.Mouse.Drag())
            {
                _rotation.Y -= Input.Mouse.Delta.X * RotationSpeed * MouseSensibility.Y * Time.DeltaTime;
                _rotation.X += Input.Mouse.Delta.Y * RotationSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (Input.Mouse.Drag(Inputs.MouseButton.Middle))
            {
                _translation.Y += Input.Mouse.Delta.Y * MoveSpeed * MouseSensibility.Y * Time.DeltaTime;
                _translation.X += Input.Mouse.Delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }
        }

        private void UpdateGamepadInput()
        {
            _translation.Z += Input.Gamepad.LeftStickValue().Y * MoveSpeed * Time.DeltaTime;
            _translation.X -= Input.Gamepad.LeftStickValue().X * StrafeSpeed * Time.DeltaTime;

            _rotation.X -= Input.Gamepad.RightStickValue().Y * LookSpeed * Time.DeltaTime;
            _rotation.Y -= Input.Gamepad.RightStickValue().X * RotationSpeed * Time.DeltaTime;

            if (Input.Gamepad.LeftShoulder())
                _translation.Y -= MoveSpeed / 2 * Time.DeltaTime;
            else if (Input.Gamepad.RightShoulder())
                _translation.Y += MoveSpeed / 2 * Time.DeltaTime;
        }
    }
}
