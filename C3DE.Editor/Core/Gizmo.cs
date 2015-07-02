using C3DE.Components.Renderers;
using C3DE.Editor.MonoGameBridge;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs.Meshes;
using C3DE.Utils;
using Microsoft.Xna.Framework;

namespace C3DE.Editor.Core
{
    public class Gizmo : SceneObject
    {
        private MeshPrefab<CylinderGeometry>[] _gizmos;

        public override void Initialize()
        {
            base.Initialize();
            Build();
        }

        public void Build()
        {
            var scene = Scene;

            var cylinderSize = new Vector3(0.025f, 2.0f, 0.025f);

            _gizmos = new MeshPrefab<CylinderGeometry>[3];
            _gizmos[0] = new MeshPrefab<CylinderGeometry>();
            _gizmos[0].Renderer.Geometry.Size = cylinderSize;
            _gizmos[0].Transform.Rotation = new Vector3(MathHelper.PiOver2, 0, 0);
            //_gizmos[0].Transform.Position = new Vector3(2.1f, 0, 0);
            _gizmos[0].Renderer.Material = new UnlitMaterial(scene);
            _gizmos[0].Renderer.Material.MainTexture = GraphicsHelper.CreateTexture(Color.DarkRed, 32, 32);

            _gizmos[1] = new MeshPrefab<CylinderGeometry>();
            _gizmos[1].Renderer.Geometry.Size = cylinderSize;
            //_gizmos[1].Transform.Position = new Vector3(0, 0, 2.1f);
            _gizmos[1].Transform.Rotation = new Vector3(0, 0, MathHelper.PiOver2);
            _gizmos[1].Renderer.Material = new UnlitMaterial(scene);
            _gizmos[1].Renderer.Material.MainTexture = GraphicsHelper.CreateTexture(Color.DarkGreen, 32, 32);

            _gizmos[2] = new MeshPrefab<CylinderGeometry>();
            _gizmos[2].Renderer.Geometry.Size = cylinderSize;
            //_gizmos[2].Transform.Position = new Vector3(0, 1.9f, 0);
            _gizmos[2].Renderer.Material = new UnlitMaterial(scene);
            _gizmos[2].Renderer.Material.MainTexture = GraphicsHelper.CreateTexture(Color.DarkBlue, 32, 32);

            foreach (var gizmo in _gizmos)
            {
                gizmo.Tag = C3DEGameHost.EditorTag;
                gizmo.Renderer.Geometry.Generate();
                scene.RenderList.Add(gizmo.GetComponent<MeshRenderer>());
            }
        }

        public override void Update()
        {
            base.Update();

            foreach (var gizmo in _gizmos)
            {
                gizmo.Update();
            }
        }

        public void SetVisible(bool isVisible, Vector3? position = null)
        {
            Enabled = isVisible;

            if (position.HasValue)
                transform.Position = position.Value;
        }

        public void SetPosition(Vector3 position)
        {
            transform.Position = position;
        }
    }
}
