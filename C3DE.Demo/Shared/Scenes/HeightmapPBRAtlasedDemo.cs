using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Demo.Scripts.Lighting;
using C3DE.Demo.Scripts.Viewers;
using C3DE.Demo.Scripts.VR;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes.PBR
{
    public class HeightmapPBRAtlasedDemo : BaseDemo
    {
        public HeightmapPBRAtlasedDemo() : base("Heightmap Atlased PBR Terrain") { }

        public override void Initialize()
        {
            base.Initialize();

            _directionalLight.AddComponent<LightMover>();

            var content = Application.Content;

            //
            // Create the PBR Terrain
            //

            // 1. PBR Material.
            var tMaterial = new PBRTerrainAtlasedMaterial();
            tMaterial.CreateAlbedos(
                content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_col"),
                content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_col"),
                content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_col"),
                content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_col"),
                false
            );

            tMaterial.CreateNormals(
                content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_nrm"),
                content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_nrm"),
                content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_nrm"),
                content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_nrm")
            );

            tMaterial.CreateRoughnessMetallicAO(
                TextureFactory.CreateRoughnessMetallicAO(
                    content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_rgh"), 
                    null, 
                    null),
                TextureFactory.CreateRoughnessMetallicAO(
                    content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_rgh"), 
                    null, 
                    content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_AO")),
                TextureFactory.CreateRoughnessMetallicAO(
                    content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_rgh"), 
                    null, 
                    content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_AO")),
                TextureFactory.CreateRoughnessMetallicAO(
                    content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_rgh"),
                    null,
                    null)
            );

            // Terrain
            var terrainGo = GameObjectFactory.CreateTerrain(null, new Vector2(1));
           
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = tMaterial;
            terrain.AddComponent<PBRViewer>();

            var weightMap = terrain.GenerateWeightMap();           
            tMaterial.WeightMap = weightMap;
            tMaterial.Tiling = new Vector2(128f);
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

            _vrPlayerEnabler.Position = new Vector3(0, water.Transform.Position.Y + 0.5f, 0);
        }
    }
}
