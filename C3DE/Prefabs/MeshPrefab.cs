using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace C3DE.Prefabs.Meshes
{
    [DataContract]
    public class MeshPrefab : GameObject
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

        public MeshPrefab() 
            : this("MeshPrefab", null) 
        { 
        }

        public MeshPrefab(string name)
            : this(name, null)
        {
        }

        public MeshPrefab(string name, Geometry geometry)
            : base(name)
        {
            _renderer = AddComponent<MeshRenderer>();
            _renderer.Geometry = geometry;
            _renderer.ReceiveShadow = false;
            _collider = AddComponent<BoxCollider>();

            if (geometry != null && !geometry.Built)
                geometry.Build();
        }
    }
}
