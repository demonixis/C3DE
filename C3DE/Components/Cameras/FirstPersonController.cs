using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Components.Cameras
{
    public class FirstPersonController : Component
    {
        private Camera _camera;
        private Transform _transform;
        private Matrix _rotationMatrix;
        private Vector3 _transformedReference;

        public FirstPersonController()
            : this(null)
        {
        }

        public FirstPersonController(SceneObject sceneObject)
            : base(sceneObject)
        {
            
        }

        public override void LoadContent(ContentManager content)
        {
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();
        }

        public override void Update()
        {
            _rotationMatrix = Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f);
            _transformedReference = Vector3.Transform(_camera.Reference, _rotationMatrix);
            _camera.Target = sceneObject.Transform.Position + _transformedReference;
        }

        public virtual void Translate(ref Vector3 move)
        {
            Matrix forwardMovement = Matrix.CreateFromYawPitchRoll(_transform.Rotation.Y, _transform.Rotation.X, 0.0f);
            Vector3 v = Vector3.Transform(move, forwardMovement);

            _transform.Translate(v.X, v.Y, v.Z);
        }

        public void Rotate(ref Vector3 rot)
        {
            _transform.Rotate(rot.X, rot.Y, rot.Z);
        }
    }
}
