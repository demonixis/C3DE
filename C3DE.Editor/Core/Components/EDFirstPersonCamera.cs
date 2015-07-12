using C3DE.Components;
using C3DE.Components.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Editor.Core.Components
{
    using WinInput = System.Windows.Input;

    public class EDFirstPersonCamera : Controller
    {
        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;
        protected Vector3 translation = Vector3.Zero;
        protected Vector3 rotation = Vector3.Zero;

        public EDFirstPersonCamera()
            : base()
        {
            Velocity = 0.8f;
            AngularVelocity = 0.8f;
            MoveSpeed = 0.25f;
            RotationSpeed = 0.5f;
            LookSpeed = 0.25f;
            StrafeSpeed = 1.5f;
            MouseSensibility = new Vector2(0.15f);
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
            _transformedReference = Vector3.Transform(translation, _rotationMatrix);

            // Translate and rotate
            transform.Translate(ref _transformedReference);
            transform.Rotate(ref rotation);

            // Update target
            EDRegistry.Camera.Target = transform.Position + Vector3.Transform(EDRegistry.Camera.Reference, _rotationMatrix);

            translation *= Velocity;
            rotation *= AngularVelocity;
        }

        protected override void UpdateInputs()
        {
            UpdateMouseInput();
            UpdateKeyboardInput();
        }

        protected override void UpdateKeyboardInput()
        {
            if (EDRegistry.Keys.Pressed(WinInput.Key.Up))
                translation.Z += MoveSpeed * Time.DeltaTime;

            else if (EDRegistry.Keys.Pressed(WinInput.Key.Down))
                translation.Z -= MoveSpeed * Time.DeltaTime;

            if (EDRegistry.Keys.Pressed(WinInput.Key.Left))
                translation.X += MoveSpeed * Time.DeltaTime / 2.0f;

            else if (EDRegistry.Keys.Pressed(WinInput.Key.Right))
                translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;
        }

        protected override void UpdateMouseInput()
        {
            if (EDRegistry.Mouse.Down(Inputs.MouseButton.Right))
            {
                rotation.Y -= EDRegistry.Mouse.Delta.X * LookSpeed * MouseSensibility.Y * Time.DeltaTime;
                rotation.X += EDRegistry.Mouse.Delta.Y * LookSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (EDRegistry.Mouse.Down(Inputs.MouseButton.Middle))
            {
                translation.Y += EDRegistry.Mouse.Delta.Y * StrafeSpeed * MouseSensibility.Y * Time.DeltaTime;
                translation.X += EDRegistry.Mouse.Delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            translation.Z += MoveSpeed * EDRegistry.Mouse.Wheel * Time.DeltaTime;
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
