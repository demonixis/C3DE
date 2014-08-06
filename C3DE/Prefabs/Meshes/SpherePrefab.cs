using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;

namespace C3DE.Prefabs.Meshes
{
    public class SpherePrefab : Prefab
    {
        private MeshRenderer _renderer;
        private SphereCollider _collider;

        public MeshRenderer Renderer
        {
            get { return _renderer; }
        }

        public SphereCollider Collider
        {
            get { return _collider; }
        }

        public SpherePrefab(string name, Scene scene, float size = 1.0f)
            : base(name, scene)
        {
            _renderer = AddComponent<MeshRenderer>();
            _renderer.Geometry = new CubeGeometry();
            _renderer.Geometry.Size = new Vector3(size);
            _renderer.RecieveShadow = false;
            _collider = AddComponent<SphereCollider>();
        }
    }
}
