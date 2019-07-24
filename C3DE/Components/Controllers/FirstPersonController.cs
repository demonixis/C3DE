using C3DE.Components.Controllers.Mobile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Components.Controllers
{
    /// <summary>
    /// A first person camera controller component.
    /// </summary>
    public class FirstPersonController : Controller
    {
        private bool _azertyKeyboard = false;
        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;
        private bool _lockCursor;
        private bool _virtualInputEnabled;
        protected Vector3 translation = Vector3.Zero;
        protected Vector3 rotation = Vector3.Zero;
        protected VirtualGamepad leftVirtaulStick;
        protected SwipeZone rightSwipeZone;

        /// <summary>
        /// Enable or disable the flying mode. Default is false.
        /// </summar>
        public bool Fly { get; set; }

        public bool DontUpdateOnClick { get; set; } = false;

        public bool VirtualInputEnabled
        {
            get => _virtualInputEnabled;
            set => SetVirtualInputSupport(value);
        }

        public bool LockCursor
        {
            get => _lockCursor;
            set
            {
                _lockCursor = value;
                Screen.ShowCursor = !_lockCursor;
                Screen.LockCursor = _lockCursor;
            }
        }

        /// <summary>
        /// Create a first person controller with default values.
        /// </summary>
        public FirstPersonController()
            : base()
        {
            Velocity = 0.92f;
            AngularVelocity = 0.85f;
            MoveSpeed = 1.5f;
            RotationSpeed = 0.15f;
            LookSpeed = 0.15f;
            StrafeSpeed = 0.75f;
            MouseSensibility = new Vector2(0.15f);
            GamepadSensibility = new Vector2(2.5f);
            Fly = false;
            _virtualInputEnabled = false;
            _lockCursor = false;
            _azertyKeyboard = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "fr";
        }

        public override void OnDisabled()
        {
            if (_virtualInputEnabled)
            {
                leftVirtaulStick.Enabled = false;
                rightSwipeZone.Enabled = false;
            }
        }

        public override void OnEnabled()
        {
            if (_virtualInputEnabled)
            {
                leftVirtaulStick.Enabled = true;
                rightSwipeZone.Enabled = true;
            }
        }

        public override void Start()
        {
#if ANDROID
            VirtualInputEnabled = true;
#endif
        }

        public override void Update()
        {
            base.Update();

            UpdateInputs();

            // Limits on X axis
            if (_transform.LocalRotation.X <= -MathHelper.PiOver2)
            {
                _transform.SetLocalRotation(-MathHelper.PiOver2 + 0.001f, null, null);
                rotation = Vector3.Zero;
            }
            else if (_transform.LocalRotation.X >= MathHelper.PiOver2)
            {
                _transform.SetLocalRotation(MathHelper.PiOver2 - 0.001f, null, null);
                rotation = Vector3.Zero;
            }

            _rotationMatrix = Matrix.CreateFromYawPitchRoll(_transform.LocalRotation.Y, _transform.LocalRotation.X, 0.0f);

            _transformedReference = Vector3.Transform(translation, !Fly ? Matrix.CreateRotationY(_transform.LocalRotation.Y) : _rotationMatrix);

            // Translate and rotate
            _transform.Translate(ref _transformedReference);
            _transform.Rotate(ref rotation);

            translation *= Velocity;
            rotation *= AngularVelocity;
        }

        protected override void UpdateInputs()
        {
            UpdateMouseInput();
            UpdateKeyboardInput();
            UpdateGamepadInput();
            UpdateTouchInput();
        }

        protected override void UpdateKeyboardInput()
        {
            if (Input.Keys.Up || (_azertyKeyboard ? Input.Keys.Pressed(Keys.Z) : Input.Keys.Pressed(Keys.W)))
                translation.Z -= MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down) || Input.Keys.Pressed(Keys.S))
                translation.Z += MoveSpeed * Time.DeltaTime;

            if (_azertyKeyboard ? Input.Keys.Pressed(Keys.Q) : Input.Keys.Pressed(Keys.A))
                translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.D))
                translation.X += MoveSpeed * Time.DeltaTime / 2.0f;

            if (_azertyKeyboard ? Input.Keys.Pressed(Keys.A) : Input.Keys.Pressed(Keys.Q))
                translation.Y += StrafeSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.E))
                translation.Y -= StrafeSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.Left))
                rotation.Y += RotationSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Right))
                rotation.Y -= RotationSpeed * Time.DeltaTime;

            if (Input.Keys.JustPressed(Keys.Tab))
                Fly = !Fly;
        }

        protected override void UpdateMouseInput()
        {
            if (!MouseEnabled)
                return;

            if (!_lockCursor && Input.Mouse.Drag())
            {
                rotation.Y -= Input.Mouse.Delta.X * RotationSpeed * MouseSensibility.Y * Time.DeltaTime;
                rotation.X -= Input.Mouse.Delta.Y * RotationSpeed * MouseSensibility.X * Time.DeltaTime;
            }
            else if (_lockCursor)
            {
                rotation.Y -= Input.Mouse.Delta.X * RotationSpeed * MouseSensibility.Y * Time.DeltaTime;
                rotation.X -= Input.Mouse.Delta.Y * RotationSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (Input.Mouse.Drag(Inputs.MouseButton.Middle))
            {
                translation.Y += Input.Mouse.Delta.Y * MoveSpeed * MouseSensibility.Y * Time.DeltaTime;
                translation.X += Input.Mouse.Delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }
        }

        protected override void UpdateGamepadInput()
        {
            translation.Z += Input.Gamepad.LeftStickValue().Y * GamepadSensibility.X * MoveSpeed * Time.DeltaTime;
            translation.X -= Input.Gamepad.LeftStickValue().X * GamepadSensibility.Y * StrafeSpeed * Time.DeltaTime;

            rotation.X -= Input.Gamepad.RightStickValue().Y * GamepadSensibility.Y * LookSpeed * Time.DeltaTime;
            rotation.Y -= Input.Gamepad.RightStickValue().X * GamepadSensibility.X * RotationSpeed * Time.DeltaTime;

            if (Input.Gamepad.LeftShoulder())
                translation.Y -= MoveSpeed / 2 * Time.DeltaTime;
            else if (Input.Gamepad.RightShoulder())
                translation.Y += MoveSpeed / 2 * Time.DeltaTime;
        }

        protected override void UpdateTouchInput()
        {
            if (_virtualInputEnabled)
            {
                translation.Z -= leftVirtaulStick.StickValue.Y * MoveSpeed * Time.DeltaTime * 0.75f;
                translation.X -= leftVirtaulStick.StickValue.X * StrafeSpeed * Time.DeltaTime * 0.75f;

                rotation.X += rightSwipeZone.Delta.Y * LookSpeed * Time.DeltaTime * 0.35f;
                rotation.Y -= rightSwipeZone.Delta.X * RotationSpeed * Time.DeltaTime * 0.35f;
            }
        }

        protected virtual void SetVirtualInputSupport(bool active)
        {
            if (active && leftVirtaulStick == null && rightSwipeZone == null)
            {
                leftVirtaulStick = _gameObject.AddComponent<VirtualGamepad>();
                rightSwipeZone = _gameObject.AddComponent<SwipeZone>();
            }
            else if (!active && leftVirtaulStick != null && rightSwipeZone != null)
            {
                _gameObject.RemoveComponent(leftVirtaulStick);
                _gameObject.RemoveComponent(rightSwipeZone);
            }

            _virtualInputEnabled = leftVirtaulStick != null && rightSwipeZone != null;
        }
    }
}
