using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs.Meshes;
using C3DE.Utils;
using Microsoft.Xna.Framework;

namespace C3DE.Editor.Core
{
    public class Gizmo : SceneObject
    {
        private MeshPrefab<CubeGeometry>[] _gizmos;

        public void Build(Scene scene)
        {
            _gizmos = new MeshPrefab<CubeGeometry>[3];
            _gizmos[0] = new MeshPrefab<CubeGeometry>();
            _gizmos[0].Renderer.Geometry.Size = new Vector3(2.0f, 0.1f, 0.1f);
            _gizmos[0].Transform.Position = new Vector3(2.1f, 0, 0);
            _gizmos[0].Renderer.Material = new SimpleMaterial(scene);
            _gizmos[0].Renderer.Material.MainTexture = GraphicsHelper.CreateBorderTexture(Color.DarkGray, Color.DarkRed, 32, 32, 2);

            _gizmos[1] = new MeshPrefab<CubeGeometry>();
            _gizmos[1].Renderer.Geometry.Size = new Vector3(0.1f, 0.1f, 2.0f);
            _gizmos[1].Transform.Position = new Vector3(0, 0, 2.1f);
            _gizmos[1].Renderer.Material = new SimpleMaterial(scene);
            _gizmos[1].Renderer.Material.MainTexture = GraphicsHelper.CreateBorderTexture(Color.DarkGray, Color.DarkGreen, 32, 32, 2);

            _gizmos[2] = new MeshPrefab<CubeGeometry>();
            _gizmos[2].Renderer.Geometry.Size = new Vector3(0.1f, 2.0f, 0.1f);
            _gizmos[2].Transform.Position = new Vector3(0, 1.9f, 0);
            _gizmos[2].Renderer.Material = new SimpleMaterial(scene);
            _gizmos[2].Renderer.Material.MainTexture = GraphicsHelper.CreateBorderTexture(Color.DarkGray, Color.DarkBlue, 32, 32, 2);

            foreach (var gizmo in _gizmos)
            {
                gizmo.Renderer.Geometry.Generate();
                scene.Add(gizmo);
            }
        }

        public override void Update()
        {

        }

        public void SetVisible(bool isVisible)
        {
            foreach (var gizmo in _gizmos)
                gizmo.Enabled = isVisible;
        }
    }
}
