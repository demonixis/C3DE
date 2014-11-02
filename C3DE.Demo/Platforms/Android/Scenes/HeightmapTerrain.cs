using C3DE.Components.Lights;
using C3DE.Demo.Scripts;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Components.Controllers;

namespace C3DE.Demo.Scenes
{
    public class HeightmapTerrain : Scene
    {
        public HeightmapTerrain() : base("Terrain Demo") { }

        public override void Initialize()
        {
            base.Initialize();

            // Add a camera with a FPS controller
            var camera = new CameraPrefab("camera");
            camera.AddComponent<DemoBehaviour>();
			camera.AddComponent<ControllerSwitcher>();
            Add(camera);

            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            // And a light
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            Add(lightPrefab);
            lightPrefab.Light.Direction = new Vector3(1, 1, 0);
            lightPrefab.Light.DiffuseColor = Color.LightSkyBlue;
            lightPrefab.AddComponent<DemoBehaviour>();
            lightPrefab.EnableShadows = true;

            // Finally a terrain
            var terrainMat = new TerrainMaterial(scene);

            var terrain = new TerrainPrefab("terrain");
            scene.Add(terrain);
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMat;
            terrain.Transform.Translate(-terrain.Width >> 1, -10, -terrain.Depth >> 1);
            var map = terrain.GenerateWeightMap();

            terrainMat.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMat.SandTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMat.SnowTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Snow");
            terrainMat.RockTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMat.WeightTexture = map;
            terrainMat.Tiling = new Vector2(4);

            // With water !
            var water = new WaterPrefab("water");
            scene.Add(water);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));

            // Don't miss the Skybox ;)
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);
        }
    }
}
