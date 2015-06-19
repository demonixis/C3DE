using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Editor.Components
{
    /// <summary>
    /// An orbit controller component.
    /// It allows user to move and rotate the camera around a point.
    /// </summary>
    public class EDOrbitController : Controller
    {
        private Camera _camera;
        private float _distance;
        private Vector2 _angle;
        private Vector3 _position;
        private Vector3 _target;

        /// <summary>
        /// Gets or sets the min angle on y-axis.
        /// </summary>
        public float MinAngle { get; set; }

        /// <summary>
        /// Gets or sets the max angle on y-axis.
        /// </summary>
        public float MaxAngle { get; set; }

        /// <summary>
        /// Gets or sets the min distance from the target.
        /// </summary>
        public float MinDistance { get; set; }

        /// <summary>
        /// Gets or sets the max distance from the target.
        /// </summary>
        public float MaxDistance { get; set; }

        /// <summary>
        /// Create an orbit controller with default values.
        /// </summary>
        public EDOrbitController()
            : base()
        {
            _angle = new Vector2(0.0f, -MathHelper.Pi / 6.0f);
            _distance = 35;

            MinAngle = -MathHelper.PiOver2 + 0.1f;
            MaxAngle = MathHelper.PiOver2 - 0.1f;
            MinDistance = 1.0f;
            MaxDistance = 100.0f;
            RotationSpeed = 0.25f;
            MoveSpeed = 10.0f;
            StrafeSpeed = 10f;
        }

        public override void Start()
        {
            _camera = GetComponent<Camera>();
        }

        public override void Update()
        {
            if (Input.Mouse.Down(MouseButton.Right))
            {
                _angle.X -= RotationSpeed * Input.Mouse.Delta.X * Time.DeltaTime;
                _angle.Y -= RotationSpeed * Input.Mouse.Delta.Y * Time.DeltaTime;
            }
 
            if (Input.Mouse.Down(MouseButton.Middle))
                _target.Y += StrafeSpeed * Input.Mouse.Delta.Y * Time.DeltaTime;
            
            _distance -= (Input.Mouse as EDMouseComponent).Wheel * 0.1f * MoveSpeed * Time.DeltaTime;

            CheckAngle();
            CheckDistance();

            _position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(_angle.X, _angle.Y, 0));
            _position *= _distance;
            _position += _camera.Target;

            transform.Position = _position;
            _camera.Target = _target;
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

        protected override void UpdateInputs()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateKeyboardInput()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateMouseInput()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateGamepadInput()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateTouchInput()
        {
            throw new NotImplementedException();
        }
    }
}

