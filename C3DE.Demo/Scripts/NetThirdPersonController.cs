using C3DE.Components.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Components.Controllers
{
    public class NetThirdPersonController : Controller
    {
        private Vector3 _transformedReference;
        private Vector3 _translation = Vector3.Zero;
        private Vector3 _rotation = Vector3.Zero;
        private NetworkView _ntView;

        public NetThirdPersonController()
            : base()
        {
            Velocity = 0.95f;
            AngularVelocity = 0.95f;
            MoveSpeed = 1.0f;
            RotationSpeed = 0.1f;
            LookSpeed = 0.15f;
            StrafeSpeed = 0.75f;
        }

        public override void Start()
        {
            _ntView = GetComponent<NetworkView>();
        }

        public override void Update()
        {
            if (_ntView.IsMine())
            {
                UpdateKeyboardInput();
                UpdateGamepadInput();

                _transformedReference = Vector3.Transform(_translation, Matrix.CreateRotationY(m_Transform.Rotation.Y));

                // Translate and rotate
                m_Transform.Translate(ref _transformedReference);
                m_Transform.Rotate(ref _rotation);

                _translation *= Velocity;
                _rotation *= AngularVelocity;
            }
        }

        protected override void UpdateKeyboardInput()
        {
            if (Input.Keys.Up || Input.Keys.Pressed(Keys.W))
                _translation.Z -= MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down) || Input.Keys.Pressed(Keys.S))
                _translation.Z += MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.A))
                _translation.X += MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.D))
                _translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            if (Input.Keys.Pressed(Keys.Left))
                _rotation.Y += RotationSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Right))
                _rotation.Y -= RotationSpeed * Time.DeltaTime;
        }

        protected override void UpdateGamepadInput()
        {
            _translation.Z -= Input.Gamepad.LeftStickValue().Y * MoveSpeed * Time.DeltaTime;
            _translation.X -= Input.Gamepad.LeftStickValue().X * StrafeSpeed * Time.DeltaTime;

            _rotation.X -= Input.Gamepad.RightStickValue().Y * LookSpeed * Time.DeltaTime;
            _rotation.Y -= Input.Gamepad.RightStickValue().X * RotationSpeed * Time.DeltaTime;
        }

        protected override void UpdateInputs()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateMouseInput()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateTouchInput()
        {
            throw new NotImplementedException();
        }
    }
}
