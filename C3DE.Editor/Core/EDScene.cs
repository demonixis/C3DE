using C3DE.Components;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Editor.Core.Components;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;

namespace C3DE.Editor.Core
{
    public class EDScene : Scene
    {
        internal const string EditorTag = "C3DE_Editor";

        internal CameraPrefab camera;
        internal LightPrefab light;
        internal TerrainPrefab grid;

        public EDScene(string name)
            : base(name)
        {
        }

        private void CreateEditorScene()
        {
            var defaultMaterial = new SimpleMaterial(this);
            defaultMaterial.MainTexture = GraphicsHelper.CreateBorderTexture(Color.LightSkyBlue, Color.LightGray, 64, 64, 1);
            DefaultMaterial = defaultMaterial;

            camera = new CameraPrefab("EditorCamera.Main");
            camera.Tag = EditorTag;
            camera.AddComponent<EDOrbitController>();
            Add(camera);

            light = new LightPrefab("Editor_MainLight", LightType.Directional);
            light.Tag = EditorTag;
            Add(light);
            light.Transform.Position = new Vector3(0, 15, 15);
            light.Light.Direction = new Vector3(0, 0.75f, 0.75f);

            // Grid
            var gridMaterial = new SimpleMaterial(this);
            gridMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(new Color(0.4f, 0.4f, 0.4f), new Color(0.9f, 0.9f, 0.9f), 256, 256);
            gridMaterial.Tiling = new Vector2(24.0f);
            gridMaterial.Alpha = 0.6f;

            grid = new TerrainPrefab("Editor_Grid");
            grid.Tag = EditorTag;
            grid.Flat();
            grid.Renderer.Material = gridMaterial;
            grid.Transform.Translate(-grid.Width >> 1, -1.0f, -grid.Depth / 2);
            Add(grid);

            camera.Transform.Position = new Vector3(-grid.Width >> 1, 2, -grid.Depth / 2);
        }

        protected override void RemoveAllComponents()
        {
            base.RemoveAllComponents();
        }

        protected override void RemoveAllObjects()
        {
            base.RemoveAllObjects();
        }
    }
}
