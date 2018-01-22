using C3DE.Components;
using C3DE.Components.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Editor
{
    public class EditorController : Controller
    {
        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;
        protected Vector3 translation = Vector3.Zero;
        protected Vector3 rotation = Vector3.Zero;

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
            if (m_Transform.Rotation.X <= -MathHelper.PiOver2)
            {
                m_Transform.SetRotation(-MathHelper.PiOver2 + 0.001f, null, null);
                rotation = Vector3.Zero;
            }
            else if (m_Transform.Rotation.X >= MathHelper.PiOver2)
            {
                m_Transform.SetRotation(MathHelper.PiOver2 - 0.001f, null, null);
                rotation = Vector3.Zero;
            }

            _rotationMatrix = Matrix.CreateFromYawPitchRoll(m_Transform.Rotation.Y, m_Transform.Rotation.X, 0.0f);
            _transformedReference = Vector3.Transform(translation, _rotationMatrix);

            // Translate and rotate
            m_Transform.Translate(ref _transformedReference);
            m_Transform.Rotate(ref rotation);

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
            if (Input.Keys.Pressed(Keys.Up))
                translation.Z -= MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down))
                translation.Z += MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.Left))
                translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.Right))
                translation.X += MoveSpeed * Time.DeltaTime / 2.0f;
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

                rotation.Y -= delta.X * LookSpeed * MouseSensibility.Y * Time.DeltaTime;
                rotation.X -= delta.Y * LookSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Middle))
            {
                translation.Y += delta.Y * StrafeSpeed * MouseSensibility.Y * Time.DeltaTime;
                translation.X -= delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            translation.Z -= MoveSpeed * Input.Mouse.Wheel * Time.DeltaTime;
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