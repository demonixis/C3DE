using C3DE.Components;
using C3DE.Components.Colliders;
using Microsoft.Xna.Framework;

namespace C3DE.Prefabs
{
    public class CameraPrefab : SceneObject
    {
        protected Camera camera;
        
        public Camera Camera
        {
            get { return camera; }
        }

        public CameraPrefab(string name)
            : base(name)
        {
            camera = AddComponent<Camera>();
            camera.Setup(new Vector3(0, 0, -10), new Vector3(0, 0, 0), Vector3.Up);
        }

        public void Setup(Vector3 position, Vector3 target, Vector3 upVector)
        {
            camera.Setup(position, target, upVector);
        }

        public void LookAt(Transform transform)
        {
            camera.Target = transform.Position;
        }

        public void LookAt(Vector3 position)
        {
            camera.Target = position;
        }
    }
}
