using C3DE.Components;
using C3DE.Components.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Editor.Core.Components
{
    using WinInput = System.Windows.Input;

    public class EDFirstPersonCamera : Controller
    {
        private Camera _camera;
        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;
        protected Vector3 translation = Vector3.Zero;
        protected Vector3 rotation = Vector3.Zero;

        public EDFirstPersonCamera()
            : base()
        {
            Velocity = 0.90f;
            AngularVelocity = 0.85f;
            MoveSpeed = 0.5f;
            RotationSpeed = 0.35f;
            LookSpeed = 0.15f;
            StrafeSpeed = 0.75f;
            MouseSensibility = new Vector2(0.15f);
            GamepadSensibility = new Vector2(2.5f);
        }

        public override void Start()
        {
            _camera = GetComponent<Camera>(); // TODO Editor.Camera
        }

        public override void Update()
        {
            UpdateInputs();

            // Limits on X axis
            if (transform.Rotation.X <= -MathHelper.PiOver2)
            {
                transform.SetRotation(-MathHelper.PiOver2 + 0.001f, null, null);
                rotation = Vector3.Zero;
            }
            else if (transform.Rotation.X >= MathHelper.PiOver2)
            {
                transform.SetRotation(MathHelper.PiOver2 - 0.001f, null, null);
                rotation = Vector3.Zero;
            }

            _rotationMatrix = Matrix.CreateFromYawPitchRoll(transform.Rotation.Y, transform.Rotation.X, 0.0f);

            _transformedReference = Vector3.Transform(translation, Matrix.CreateRotationY(transform.Rotation.Y));

            // Translate and rotate
            transform.Translate(ref _transformedReference);
            transform.Rotate(ref rotation);

            // Update target
            _camera.Target = transform.Position + Vector3.Transform(_camera.Reference, _rotationMatrix);

            translation *= Velocity;
            rotation *= AngularVelocity; Debug.Log(translation.X);
        }

        protected override void UpdateInputs()
        {
            UpdateMouseInput();
            UpdateKeyboardInput();
            UpdateGamepadInput();
        }

        protected override void UpdateKeyboardInput()
        {
            if (Registry.Keys.Pressed(WinInput.Key.Up))
                translation.Z += MoveSpeed * Time.DeltaTime;

            else if (Registry.Keys.Pressed(WinInput.Key.Down))
                translation.Z -= MoveSpeed * Time.DeltaTime;

            if (Registry.Keys.Pressed(WinInput.Key.Left))
                translation.X += MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Registry.Keys.Pressed(WinInput.Key.Right))
                translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;
        }

        protected override void UpdateMouseInput()
        {
            if (Registry.Mouse.Down(Inputs.MouseButton.Right))
            {
                rotation.Y -= Registry.Mouse.Delta.X * RotationSpeed * MouseSensibility.Y * Time.DeltaTime;
                rotation.X += Registry.Mouse.Delta.Y * RotationSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (Registry.Mouse.Down(Inputs.MouseButton.Middle))
            {
                translation.Y += Registry.Mouse.Delta.Y * MoveSpeed * MouseSensibility.Y * Time.DeltaTime;
                translation.X += Registry.Mouse.Delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            translation.Z += MoveSpeed * Registry.Mouse.Wheel;
        }

        protected override void UpdateGamepadInput()
        {
        }

        protected override void UpdateTouchInput()
        {
        }

        protected virtual void SetVirtualInputSupport(bool active)
        {
        }
    }
}
