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
        public Vector3 AngularVelocity { get; set; }
        public float MoveSpeed { get; set; }
        public float RotationSpeed { get; set; }
        public float LookSpeed { get; set; }
        public float StrafeSpeed { get; set; }
        public bool FourAxis { get; set; }

        public FirstPersonController()
            : this(null)
        {
        }

        public FirstPersonController(SceneObject sceneObject)
            : base(sceneObject)
        {
            Velocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            MoveSpeed = 0.01f;
            RotationSpeed = 0.005f;
            LookSpeed = 0.01f;
            StrafeSpeed = 0.01f;
            FourAxis = false;
        }

        public override void LoadContent(ContentManager content)
        {
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();
        }

        public override void Update()
        {
            if (Input.Keys.Up || Input.Keys.Pressed(Keys.W))
                _translation.Z += MoveSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Down) || Input.Keys.Pressed(Keys.S))
                _translation.Z -= MoveSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.A))
                _translation.X += MoveSpeed * Time.DeltaTime / 2.0f;

            else if (Input.Keys.Pressed(Keys.D))
                _translation.X -= MoveSpeed * Time.DeltaTime / 2.0f;

            if (Input.Keys.Pressed(Keys.Q))
                _translation.Y += StrafeSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.E))
                _translation.Y -= StrafeSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.PageUp))
                _rotation.X -= LookSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.PageDown))
                _rotation.X += LookSpeed * Time.DeltaTime;

            if (Input.Keys.Pressed(Keys.Left))
                _rotation.Y += RotationSpeed * Time.DeltaTime;

            else if (Input.Keys.Pressed(Keys.Right))
                _rotation.Y -= RotationSpeed * Time.DeltaTime;

            _rotationMatrix = Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f);

            _transformedReference = Vector3.Transform(_translation, !FourAxis ? Matrix.CreateRotationY(_transform.Rotation.Y) : _rotationMatrix);

            // Translate and rotate
            _transform.Translate(ref _transformedReference);
            _transform.Rotate(ref _rotation);

            // Update target
            _camera.Target = sceneObject.Transform.Position + Vector3.Transform(_camera.Reference, _rotationMatrix);

            _translation *= Velocity;
            _rotation *= AngularVelocity;
        }
    }
}
