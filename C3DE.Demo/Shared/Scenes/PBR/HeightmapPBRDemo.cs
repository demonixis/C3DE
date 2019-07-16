using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
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

            //SpawnRadialLights(5, 0.0f, 16, 0.5f, 0.5f);
            //SpawnRadialLights(25, 0.0f, 16, 0.5f, 0.5f);
            //SpawnRadialLights(50, 0.0f, 16, 0.5f, 0.5f);

            var content = Application.Content;

            // Finally a terrain
            var terrainMaterial = new PBRTerrainMaterial();
            terrainMaterial.GrassMap = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_col");
            terrainMaterial.GrassNormalMap = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_nrm");
            terrainMaterial.SnowMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_col");
            terrainMaterial.SandNormalMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_nrm");
            terrainMaterial.SnowMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_col");
            terrainMaterial.SnowNormalMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_nrm");
            terrainMaterial.RockMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_col");
            terrainMaterial.RockNormalMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_nrm");

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMaterial;

            var weightMap = terrain.GenerateWeightMap();
            terrainMaterial.WeightMap = weightMap;
            terrainMaterial.Tiling = new Vector2(4);
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