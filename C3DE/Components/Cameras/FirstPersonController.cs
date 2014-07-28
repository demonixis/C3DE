using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Components.Cameras
{
    public class FirstPersonController : Component
    {
        private Camera _camera;
        private Transform _transform;
        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;
        private Vector3 _translation = Vector3.Zero;
        private Vector3 _rotation = Vector3.Zero;
        public Vector3 Velocity { get; set; }
        public float MoveSpeed { get; set; }
        public float RotationSpeed { get; set; }
        public float StrafeSpeed { get; set; }
        public bool FourAxis { get; set; }

        public FirstPersonController()
            : this(null)
        {
        }

        public FirstPersonController(SceneObject sceneObject)
            : base(sceneObject)
        {
            Velocity = Vector3.One;
            MoveSpeed = 0.05f;
            RotationSpeed = 0.05f;
            StrafeSpeed = 0.05f;
            FourAxis = true;
        }

        public override void LoadContent(ContentManager content)
        {
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();
        }

        public override void Update()
        {
            _translation = Vector3.Zero;
            _rotation = Vector3.Zero;

            if (Input.Keys.Up || Input.Keys.Pressed(Keys.W))
                _translation.Z += 0.1f;

            else if (Input.Keys.Pressed(Keys.Down) || Input.Keys.Pressed(Keys.S))
                _translation.Z -= 0.1f;

            if (Input.Keys.Pressed(Keys.A))
                _translation.X += 0.1f;

            else if (Input.Keys.Pressed(Keys.D))
                _translation.X -= 0.1f;

            if (Input.Keys.Pressed(Keys.Q))
                _translation.Y += 0.1f;

            else if (Input.Keys.Pressed(Keys.E))
                _translation.Y -= 0.1f;

            if (Input.Keys.Pressed(Keys.PageUp))
                _rotation.X -= 0.1f;

            else if (Input.Keys.Pressed(Keys.PageDown))
                _rotation.X += 0.1f;

            if (Input.Keys.Pressed(Keys.Left))
                _rotation.Y += 0.1f;

            else if (Input.Keys.Pressed(Keys.Right))
                _rotation.Y -= 0.1f;

            _rotationMatrix = Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f);

            _translation *= Time.DeltaTime * MoveSpeed * Velocity;
            _rotation *= Time.DeltaTime * RotationSpeed * Velocity;

            _transformedReference = Vector3.Transform(_translation, _rotationMatrix);

            // Translate and rotate
            _transform.Translate(ref _transformedReference);
            _transform.Rotate(ref _rotation);

            // Update target
            _camera.Target = sceneObject.Transform.Position + Vector3.Transform(_camera.Reference, _rotationMatrix);
        }
    }
}
