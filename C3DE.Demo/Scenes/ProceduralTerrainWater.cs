using C3DE.Components.Lights;
using C3DE.Demo.Scripts;
using C3DE.Materials;
using C3DE.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainWater : Scene
    {
        public ProceduralTerrainWater() : base("Procedural Terrain + Water") { }  

        public override void Initialize()
        {
            base.Initialize();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // Camera
            var camera = new CameraPrefab("camera");
            camera.AddComponent<ControllerSwitcher>();
            camera.AddComponent<DemoBehaviour>();
            Add(camera);

            // Light
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            lightPrefab.Transform.Translate(0, 10, 0);
            lightPrefab.Light.Range = 25;
            lightPrefab.Light.Intensity = 1.0f;
            lightPrefab.Light.FallOf = 5f;
            lightPrefab.Light.Direction = new Vector3(0, 0.5f, 1);
            lightPrefab.Light.Angle = MathHelper.PiOver4;
            lightPrefab.Light.ShadowGenerator.ShadowStrength = 0.6f;
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            Add(lightPrefab);

            // Finally a terrain
            var terrainMat = new TerrainMaterial(scene);

            var terrain = new Terrain("terrain");
            terrain.Randomize(4, 12);
            terrain.Renderer.Material = terrainMat;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            terrain.SetWeightData(0.5f, 4, 15, 30);

            terrainMat.Texture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMat.SandTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMat.SnowTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Snow");
            terrainMat.RockTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMat.WeightTexture = terrain.GenerateWeightMap();
            terrainMat.Tiling = new Vector2(4);

            // Water !
            var water = new WaterPrefab("water");
            Add(water);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));

            var waterMat = water.Renderer.Material as WaterMaterial;
            waterMat.ReflectiveMap = scene.RenderSettings.Skybox.Texture;
            waterMat.WaterTransparency = 0.6f;
        }
    }
}
