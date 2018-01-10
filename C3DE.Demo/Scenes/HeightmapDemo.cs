using C3DE.Components;
using C3DE.Components.Lighting;
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
            m_DirectionalLight.AddComponent<LightSwitcher>().LogPositionRotation = false;
            m_DirectionalLight.ShadowGenerator.ShadowStrength = 0.5f;

            // Finally a terrain
            var terrainMaterial = new StandardTerrainMaterial(m_Scene);
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

            // Sun Flares
            var glowTexture = content.Load<Texture2D>("Textures/Flares/glow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/flare1"),
                content.Load<Texture2D>("Textures/Flares/flare2"),
                content.Load<Texture2D>("Textures/Flares/flare3")
            };

            var direction = m_DirectionalLight.Transform.LocalRotation;
            direction.Normalize();

            var sunflares = m_Camera.AddComponent<LensFlare>();
            sunflares.LightDirection = direction; 
            sunflares.Setup(glowTexture, flareTextures);

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.Exp2;
            RenderSettings.Skybox.FogSupported = true;
            RenderSettings.Skybox.OverrideSkyboxFog(FogMode.Exp2, 0.05f, 0, 0);
        }
    }
}
