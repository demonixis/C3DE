using C3DE.Components;
using C3DE.Components.Colliders;
using Microsoft.Xna.Framework;

namespace C3DE.Prefabs
{
    public class CameraPrefab : Prefab
    {
        protected Camera camera;
        protected SphereCollider collider;
        
        public Camera Camera
        {
            get { return camera; }
        }

        public SphereCollider Collider
        {
            get { return collider; }
        }

        public CameraPrefab(string name, Scene scene)
            : base(name, scene)
        {
            camera = AddComponent<Camera>();
            collider = AddComponent<SphereCollider>();

            camera.Setup(new Vector3(0, 0, -10), new Vector3(0, 0, 0), Vector3.Up);
            collider.Sphere = new BoundingSphere(transform.Position, 2.5f);
            collider.IsPickable = false;
        }

        public void Setup(Vector3 position, Vector3 target, Vector3 upVector)
        {
            camera.Setup(position, target, upVector);
        }
    }
}
