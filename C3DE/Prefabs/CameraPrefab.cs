using C3DE.Components.Cameras;
using Microsoft.Xna.Framework;

namespace C3DE.Prefabs
{
    public class CameraPrefab : SceneObject
    {
        private Camera _camera;
        
        public Camera Camera
        {
            get { return _camera; }
        }

        public CameraPrefab()
            : this(null)
        {
        }

        public CameraPrefab(string name)
            : base()
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;

            _camera = AddComponent<Camera>();
            _camera.Setup(new Vector3(0, 0, -10), new Vector3(0, 0, 0), Vector3.Up);
        }

        public void Setup(Vector3 position, Vector3 target, Vector3 upVector)
        {
            _camera.Setup(new Vector3(0, 0, 0), new Vector3(0, 0, 0), Vector3.Up);
        }
    }
}
