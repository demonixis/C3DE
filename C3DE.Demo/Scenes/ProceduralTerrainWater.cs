using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainWater : Scene
    {
        public ProceduralTerrainWater() : base("Procedural Terrain (Water)") { }  

        public override void Initialize()
        {
            base.Initialize();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // And a camera with some components
            var cameraGo = GameObjectFactory.CreateCamera();
            cameraGo.AddComponent<ControllerSwitcher>();
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);

            // A light is required to illuminate objects.
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional);
            lightGo.Transform.Rotation = new Vector3(-1, 1, 0);
            Add(lightGo);

            // Finally a terrain
            var terrainMaterial = new TerrainMaterial(scene);
            terrainMaterial.Texture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMaterial.SandTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMaterial.SnowTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Snow");
            terrainMaterial.RockTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();

            terrain.Randomize(4, 12);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrainGo);

            terrain.SetWeightData(0.5f, 4, 15, 30);

            
            terrainMaterial.WeightTexture = terrain.GenerateWeightMap();
            terrainMaterial.Tiling = new Vector2(4);

            // Water
            var waterTexture = Application.Content.Load<Texture2D>("Textures/water");
            var lavaNormal = Application.Content.Load<Texture2D>("Textures/wavesbump");
            var waterGo = GameObjectFactory.CreateWater(waterTexture, lavaNormal, new Vector3(terrain.Width * 0.5f));
            Add(waterGo);

            var water = waterGo.GetComponent<MeshRenderer>();
            var waterMat = (WaterMaterial)water.Material;
            waterMat.ReflectiveMap = scene.RenderSettings.Skybox.Texture;
            waterMat.WaterTransparency = 0.6f;
        }
    }
}
