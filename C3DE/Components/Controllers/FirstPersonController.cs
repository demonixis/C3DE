using C3DE.Components.Controllers.Mobile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Components.Controllers
{
    /// <summary>
    /// A first person camera controller component.
    /// </summary>
    public class FirstPersonController : Controller
    {
        private Camera _camera;
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
        /// </summary>
        public bool Fly { get; set; }

        public bool VirtualInputEnabled
        {
            get { return _virtualInputEnabled; }
            set
            {
                SetVirtualInputSupport(value);
            }
        }

        public bool LockCursor
        {
            get { return _lockCursor; }
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
            Fly = false;
            _virtualInputEnabled = false;
            _lockCursor = false;
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
            _camera = GetComponent<Camera>();

            if (_camera == null)
                throw new Exception("No camera attached to this scene object.");

#if ANDROID
                VirtualInputEnabled = true;
#endif
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

            _transformedReference = Vector3.Transform(translation, !Fly ? Matrix.CreateRotationY(transform.Rotation.Y) : _rotationMatrix);

            // Translate and rotate
            transform.Translate(ref _transformedReference);
            transform.Rotate(ref rotation);

            // Update target
            _camera.Target = transform.Position + Vector3.Transform(_camera.Reference, _rotationMatrix);

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
            if (Input.Keys.Up || Input.Keys.Pressed(Keys.W))
                translation.Z += MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down) || Input.Keys.Pressed(Keys.S))
                translation.Z -= MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.A))
                translation.X += MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.D))
                translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            if (Input.Keys.Pressed(Keys.A))
                translation.Y += StrafeSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.E))
                translation.Y -= StrafeSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.PageUp))
                rotation.X -= LookSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.PageDown))
                rotation.X += LookSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.Left))
                rotation.Y += RotationSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Right))
                rotation.Y -= RotationSpeed * Time.DeltaTime;

            if (Input.Keys.JustPressed(Keys.Tab))
                Fly = !Fly;
        }

        protected override void UpdateMouseInput()
        {
            if (!_lockCursor && Input.Mouse.Drag())
            {
                rotation.Y -= Input.Mouse.Delta.X * RotationSpeed * MouseSensibility.Y * Time.DeltaTime;
                rotation.X += Input.Mouse.Delta.Y * RotationSpeed * MouseSensibility.X * Time.DeltaTime;
            }
            else if (_lockCursor)
            {
                rotation.Y -= Input.Mouse.Delta.X * RotationSpeed * MouseSensibility.Y * Time.DeltaTime;
                rotation.X += Input.Mouse.Delta.Y * LookSpeed * MouseSensibility.X * Time.DeltaTime;
            }

            if (Input.Mouse.Drag(Inputs.MouseButton.Middle))
            {
                translation.Y += Input.Mouse.Delta.Y * MoveSpeed * MouseSensibility.Y * Time.DeltaTime;
                translation.X += Input.Mouse.Delta.X * StrafeSpeed * MouseSensibility.X * Time.DeltaTime;
            }
        }

        protected override void UpdateGamepadInput()
        {
            translation.Z += Input.Gamepad.LeftStickValue().Y * MoveSpeed * Time.DeltaTime;
            translation.X -= Input.Gamepad.LeftStickValue().X * StrafeSpeed * Time.DeltaTime;

            rotation.X -= Input.Gamepad.RightStickValue().Y * LookSpeed * Time.DeltaTime;
            rotation.Y -= Input.Gamepad.RightStickValue().X * RotationSpeed * Time.DeltaTime;

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
                leftVirtaulStick = sceneObject.AddComponent<VirtualGamepad>();
                rightSwipeZone = sceneObject.AddComponent<SwipeZone>();
            }
            else if (!active && leftVirtaulStick != null && rightSwipeZone != null)
            {
                sceneObject.RemoveComponent(leftVirtaulStick);
                sceneObject.RemoveComponent(rightSwipeZone);
            }

            _virtualInputEnabled = leftVirtaulStick != null && rightSwipeZone != null;
        }
    }
}
