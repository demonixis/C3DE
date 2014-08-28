using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;

namespace C3DE.Prefabs.Meshes
{
    public class CubePrefab : Prefab
    {
        private MeshRenderer _renderer;
        private BoxCollider _collider;

        public MeshRenderer Renderer
        {
            get { return _renderer; }
        }

        public BoxCollider Collider
        {
            get { return _collider; }
        }

        public CubePrefab(string name, Scene scene, float size = 1.0f)
            : base(name, scene)
        {
            
        }

        public override void Initialize()
        {
            base.Initialize();
            _renderer = AddComponent<MeshRenderer>();
            _renderer.Geometry = new CubeGeometry();
            _renderer.Geometry.Size = new Vector3(1);
            _renderer.ReceiveShadow = false;
            _collider = AddComponent<BoxCollider>();
            _renderer.Geometry.Generate();
        }
    }
}
