using C3DE.Components.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Editor
{
    public sealed class EditorController : Controller
    {
        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;
        private Vector3 _translation;
        private Vector3 _rotation;

        public EditorController()
            : base()
        {
            MoveSpeed = 5.0f;
            RotationSpeed = 0.45f;
            LookSpeed = 0.25f;
            StrafeSpeed = 1.5f;
            MouseSensibility = new Vector2(1.0f);
        }

        public override void Update()
        {
            UpdateInputs();

            // Limits on X axis
            if (_transform.Rotation.X <= -MathHelper.PiOver2)
            {
                _transform.SetLocalRotation(-MathHelper.PiOver2 + 0.001f, null, null);
                _rotation = Vector3.Zero;
            }
            else if (_transform.Rotation.X >= MathHelper.PiOver2)
            {
                _transform.SetLocalRotation(MathHelper.PiOver2 - 0.001f, null, null);
                _rotation = Vector3.Zero;
            }

            _rotationMatrix = Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f);
            _transformedReference = Vector3.Transform(_translation, _rotationMatrix);

            // Translate and rotate
            _transform.Translate(ref _transformedReference);
            _transform.Rotate(ref _rotation);

            _translation *= Velocity;
            _rotation *= AngularVelocity;
        }

        protected override void UpdateInputs()
        {
            UpdateMouseInput();
            UpdateKeyboardInput();
        }

        protected override void UpdateKeyboardInput()
        {
            if (Input.Keys.Pressed(Keys.Up))
                _translation.Z -= MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down))
                _translation.Z += MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.Left))
                _translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.Right))
                _translation.X += MoveSpeed * Time.DeltaTime / 2.0f;
        }

        protected override void UpdateMouseInput()
        {
            var delta = Input.Mouse.Delta;

            if (Input.Mouse.Down(Inputs.MouseButton.Right))
            {
                if (Input.Mouse.JustClicked(Inputs.MouseButton.Right))
                {
                    if (System.Math.Abs(delta.X) > 2)
                        delta.X = 0;

                    if (System.Math.Abs(delta.Y) > 2)
                        delta.Y = 0;
                }

                _rotation.Y -= delta.X * LookSpeed * MouseSensibility.Y * Time.DeltaTime;
                _rotation.X -= delta.Y * LookSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Middle))
            {
                _translation.Y += delta.Y * StrafeSpeed * MouseSensibility.Y * Time.DeltaTime;
                _translation.X -= delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            _translation.Z -= MoveSpeed * 0.05f * Input.Mouse.Wheel * Time.DeltaTime;
        }

        protected override void UpdateGamepadInput()
        {
        }

        protected override void UpdateTouchInput()
        {
        }
    }
}