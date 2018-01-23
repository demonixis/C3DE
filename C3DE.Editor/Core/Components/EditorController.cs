using C3DE.Components.Controllers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Editor
{
    public sealed class EditorController : Controller
    {
        private Matrix m_RotationMatrix;
        private Vector3 m_TransformedReference;
        private Vector3 m_Translation;
        private Vector3 m_Rotation;

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
                m_Rotation = Vector3.Zero;
            }
            else if (m_Transform.Rotation.X >= MathHelper.PiOver2)
            {
                m_Transform.SetRotation(MathHelper.PiOver2 - 0.001f, null, null);
                m_Rotation = Vector3.Zero;
            }

            m_RotationMatrix = Matrix.CreateFromYawPitchRoll(m_Transform.Rotation.Y, m_Transform.Rotation.X, 0.0f);
            m_TransformedReference = Vector3.Transform(m_Translation, m_RotationMatrix);

            // Translate and rotate
            m_Transform.Translate(ref m_TransformedReference);
            m_Transform.Rotate(ref m_Rotation);

            m_Translation *= Velocity;
            m_Rotation *= AngularVelocity;
        }

        protected override void UpdateInputs()
        {
            UpdateMouseInput();
            UpdateKeyboardInput();
        }

        protected override void UpdateKeyboardInput()
        {
            if (Input.Keys.Pressed(Keys.Up))
                m_Translation.Z -= MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down))
                m_Translation.Z += MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.Left))
                m_Translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.Right))
                m_Translation.X += MoveSpeed * Time.DeltaTime / 2.0f;
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

                m_Rotation.Y -= delta.X * LookSpeed * MouseSensibility.Y * Time.DeltaTime;
                m_Rotation.X -= delta.Y * LookSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Middle))
            {
                m_Translation.Y += delta.Y * StrafeSpeed * MouseSensibility.Y * Time.DeltaTime;
                m_Translation.X -= delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            m_Translation.Z -= MoveSpeed * 0.05f * Input.Mouse.Wheel * Time.DeltaTime;
        }

        protected override void UpdateGamepadInput()
        {
        }

        protected override void UpdateTouchInput()
        {
        }
    }
}