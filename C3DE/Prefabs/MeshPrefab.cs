using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;

namespace C3DE.Prefabs.Meshes
{
    public class MeshPrefab<T> : SceneObject where T : Geometry, new()
    {
        private MeshRenderer _renderer;
        private Collider _collider;

        public MeshRenderer Renderer
        {
            get { return _renderer; }
        }

        public Collider Collider
        {
            get { return _collider; }
        }

        public MeshPrefab(string name, float size = 1.0f)
            : base(name)
        {
            _renderer = AddComponent<MeshRenderer>();
            _renderer.Geometry = new T();
            _renderer.Geometry.Size = new Vector3(1);
            _renderer.ReceiveShadow = false;
            _collider = AddComponent<SphereCollider>();
            _renderer.Geometry.Generate();
        }
    }
}
