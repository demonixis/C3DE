using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
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

            m_DirectionalLight.AddComponent<LightMover>();
            m_DirectionalLight.AddComponent<LightSwitcher>().SetBoxAlign(true);

            // Finally a terrain
            var terrainMaterial = new StandardTerrainMaterial();
            terrainMaterial.MainTexture = content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMaterial.SandTexture = content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMaterial.SnowTexture = content.Load<Texture2D>("Textures/Terrain/Snow");
            terrainMaterial.RockTexture = content.Load<Texture2D>("Textures/Terrain/Rock");

            var terrainGo = GameObjectFactory.CreateTerrain();
            
            m_Scene.Add(terrainGo);

            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMaterial;

            var weightMap = terrain.GenerateWeightMap();           
            terrainMaterial.WeightTexture = weightMap;
            terrainMaterial.Tiling = new Vector2(4);
            terrainGo.AddComponent<WeightMapViewer>();

            // With water !
            var waterTexture = content.Load<Texture2D>("Textures/water");
            var bumpTexture = content.Load<Texture2D>("Textures/wavesbump");
            var water = GameObjectFactory.CreateWater(waterTexture, bumpTexture, new Vector3(terrain.Width * 0.5f));
            water.Transform.Translate(0, 10.0f, 0);
            m_Scene.Add(water);

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.None;
            RenderSettings.Skybox.FogSupported = true;
            RenderSettings.Skybox.OverrideSkyboxFog(FogMode.Exp2, 0.05f, 0, 0);

            var vrPlayerEnabler = m_Camera.AddComponent<VRPlayerEnabler>();
            vrPlayerEnabler.Position = new Vector3(0, water.Transform.Position.Y + 0.5f, 0);
        }
    }
}
