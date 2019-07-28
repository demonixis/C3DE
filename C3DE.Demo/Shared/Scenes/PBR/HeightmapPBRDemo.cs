using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Demo.Scripts.Viewers;
using C3DE.Demo.Scripts.VR;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class HeightmapPBRDemo : SimpleDemo
    {
        public HeightmapPBRDemo() : base("Heightmap PBR Terrain") { }

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;
            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = GetTerrainMaterial(content, terrain.GenerateWeightMap(), true);
            terrainGo.AddComponent<WeightMapViewer>();

            //
            // PBR Water
            //
            var water = GameObjectFactory.CreateWater(null, null, new Vector3(terrain.Width * 0.5f));
            water.Transform.Translate(0, 10.0f, 0);

            var waterMaterial = new PBRWaterMaterial();
            waterMaterial.MainTexture = content.Load<Texture2D>("Textures/Fluids/water");
            waterMaterial.NormalMap = content.Load<Texture2D>("Textures/Fluids/wavesbump");
            waterMaterial.CreateRoughnessMetallicAO(0.0f, 0.0f, 1.0f);

            var waterRenderer = water.GetComponent<Renderer>();
            waterRenderer.Material = waterMaterial;
            waterMaterial.Tiling = new Vector2(0.5f);

            //
            // Environment.
            //
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.NatureSkybox, 256);
            RenderSettings.FogMode = FogMode.None;

            var vrPlayerEnabler = _camera.AddComponent<VRPlayerEnabler>();
            vrPlayerEnabler.Position = new Vector3(0, water.Transform.Position.Y + 0.5f, 0);
        }
    }
}