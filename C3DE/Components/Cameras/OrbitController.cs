using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Components.Cameras
{
    public class OrbitController : Component
    {
        private Camera _camera;
        private Transform _transform;

        private float _distance;
        private Vector2 _angle;
        private Vector3 _position;
        private Vector3 _target;

        public OrbitController()
            : this(null)
        {
        }

        public OrbitController(SceneObject sceneObject)
            : base(sceneObject)
        {
            _angle = Vector2.Zero;
            _distance = 25;
        }

        public override void LoadContent(ContentManager content)
        {
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();
        }

        public override void Update()
        {
            UpdateKeyboardInput();
            UpdateMouseInput();
            UpdateGamepadInput();

            _position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(_angle.X, _angle.Y, 0));
            _position *= _distance;
            _position += _camera.Target;

            _transform.Position = _position;
            _camera.Target = _target;
        }

        private void UpdateKeyboardInput()
        {
            if (Input.Keys.Pressed(Keys.PageUp))
                _distance += 1f;
            else if (Input.Keys.Pressed(Keys.PageUp))
                _distance -= 1f;

            if (Input.Keys.Up)
                _angle.Y -= 0.1f;
            else if (Input.Keys.Down)
                _angle.Y += 0.1f;

            if (Input.Keys.Left)
                _angle.X -= 0.1f;
            else if (Input.Keys.Right)
                _angle.X += 0.1f;
        }

        private void UpdateMouseInput()
        {
            if (Input.Mouse.Down(Inputs.MouseButton.Left) && Input.Mouse.Drag())
            {
                _angle.X -= 0.01f * Input.Mouse.Delta.X;
                _angle.Y -= 0.01f * Input.Mouse.Delta.Y;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Right))
            {
                _target.X += 0.1f * Input.Mouse.Delta.X;
                _target.Y += 0.1f * Input.Mouse.Delta.Y;
            }

            if (Input.Mouse.Down(Inputs.MouseButton.Middle))
                _target.Y += 0.1f * Input.Mouse.Delta.Y;

            _distance -= Input.Mouse.Wheel * 0.01f;
        }

        private void UpdateGamepadInput()
        {
            _angle += Input.Gamepad.LeftStickValue() * 0.05f;

            _target.X += Input.Gamepad.RightStickValue().X * 0.1f;
            _target.Y += Input.Gamepad.RightStickValue().Y * 0.1f;

            if (Input.Gamepad.LeftShoulder())
                _distance += 0.1f;
            else if (Input.Gamepad.RightShoulder())
                _distance -= 0.1f;
        }
    }
}
