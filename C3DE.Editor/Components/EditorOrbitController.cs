using C3DE.Components;
using C3DE.Components.Controllers;
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
    public class EditorOrbitController : Controller
    {
        private Camera _camera;
        private float _distance;
        private Vector2 _angle;
        private Vector3 _position;
        private Vector3 _target;
        private Vector2 _angleVelocity;
        private Vector3 _positionVelicoty;
        private Vector3 _cacheVec3;
        private float _distanceVelocity;

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
        public EditorOrbitController()
            : base()
        {
            _angle = new Vector2(0.0f, -MathHelper.Pi / 6.0f);
            _distance = 35;

            MinAngle = -MathHelper.PiOver2 + 0.1f;
            MaxAngle = MathHelper.PiOver2 - 0.1f;
            MinDistance = 1.0f;
            MaxDistance = 100.0f;
            RotationSpeed = 0.05f;
            MoveSpeed = 2.0f;
            StrafeSpeed = 1.75f;
            GamepadSensibility = 2.5f;
            Velocity = 0.95f;
            AngularVelocity = 0.90f;
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

            _angle += _angleVelocity;
            _distance += _distanceVelocity;
            _target += _positionVelicoty;

            CheckAngle();
            CheckDistance();

            _position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(_angle.X, _angle.Y, 0));
            _position *= _distance;
            _position += _camera.Target;

            transform.Position = _position;
            _camera.Target = _target;

            _angleVelocity *= AngularVelocity;
            _distanceVelocity *= Velocity;
            _positionVelicoty *= Velocity;
        }

        private void UpdateMouseInput()
        {
            if (Input.Mouse.Down(Inputs.MouseButton.Left))
            {
                _angleVelocity.X -= RotationSpeed * Input.Mouse.Delta.X * Time.DeltaTime;
                _angleVelocity.Y -= RotationSpeed * Input.Mouse.Delta.Y * Time.DeltaTime;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Right))
            {
                _positionVelicoty.X += StrafeSpeed * Input.Mouse.Delta.X * Time.DeltaTime;
                _positionVelicoty.Y += StrafeSpeed * Input.Mouse.Delta.Y * Time.DeltaTime;
            }

            _distanceVelocity -= (Input.Mouse as EditorMouseComponent).Wheel * 0.01f * MoveSpeed * Time.DeltaTime;
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
    }
}

