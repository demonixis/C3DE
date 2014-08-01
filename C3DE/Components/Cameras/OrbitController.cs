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

        public OrbitController()
            : this(null)
        {
        }

        public OrbitController(SceneObject sceneObject)
            : base(sceneObject)
        {
            _angle = Vector2.Zero;
            _distance = 10;
        }

        public override void LoadContent(ContentManager content)
        {
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();
        }

        public override void Update()
        {
            // TODO add a needUpdate flag

            if (Input.Mouse.Drag())
            {
                _angle.X -= 0.01f * Input.Mouse.Delta.X;
                _angle.Y -= 0.01f * Input.Mouse.Delta.Y;
            }

            if (Input.Keys.Pressed(Keys.PageUp))
                _distance += 1f;
            else if (Input.Keys.Pressed(Keys.PageDown))
                _distance -= 1f;

            if (Input.Mouse.Wheel != 0.0)
                _distance -= Input.Mouse.Wheel * 0.01f;

            var position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(_angle.X, _angle.Y, 0));
            position *= _distance;
            position += _camera.Target;

            _transform.Position = position;
        }
    }
}
