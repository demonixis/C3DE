using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class HeightmapDemo : SimpleDemo
    {
        public HeightmapDemo() : base("Heightmap Terrain") { }

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            // Finally a terrain
            var tMaterial = new PBRTerrainMaterial();
            tMaterial.CreateAlbedos(
                content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_col"),
                content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_col"),
                content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_col"),
                content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_col")
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

            var terrainGo = GameObjectFactory.CreateTerrain(null, new Vector2(1));
           
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = tMaterial;
            terrain.AddComponent<PBRViewer>();

            var weightMap = terrain.GenerateWeightMap();           
            tMaterial.WeightMap = weightMap;
            tMaterial.Tiling = new Vector2(128f);
            terrainGo.AddComponent<WeightMapViewer>();

            // With water !
            var waterTexture = content.Load<Texture2D>("Textures/Fluids/water");
            var bumpTexture = content.Load<Texture2D>("Textures/Fluids/wavesbump");
            var water = GameObjectFactory.CreateWater(waterTexture, bumpTexture, new Vector3(terrain.Width * 0.5f));
            water.Transform.Translate(0, 10.0f, 0);
            water.GetComponent<Renderer>().Enabled = false;
            var waterMaterial = (StandardWaterMaterial)water.GetComponent<Renderer>().Material;
            waterMaterial.Tiling = new Vector2(0.5f);
            waterMaterial.Shininess = 5;

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.None;
            RenderSettings.Skybox.FogSupported = true;
            RenderSettings.Skybox.OverrideSkyboxFog(FogMode.Exp2, 0.05f, 0, 0);

            var vrPlayerEnabler = _camera.AddComponent<VRPlayerEnabler>();
            vrPlayerEnabler.Position = new Vector3(0, water.Transform.Position.Y + 0.5f, 0);
        }
    }
}
