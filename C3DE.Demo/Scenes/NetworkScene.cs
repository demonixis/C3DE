using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Demo.Scripts;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class NetworkScene : Scene
    {
        public NetworkScene() : base("Network demo") { }

        public override void Initialize()
        {
            base.Initialize();

            // Camera
            var camera = new CameraPrefab("camera");
            Add(camera);
            camera.AddComponent<OrbitController>();
            camera.AddComponent<NetworkManager>();

            // Light
            var lightPrefab = new LightPrefab("lightPrefab", LightType.Point);
            Add(lightPrefab);
            lightPrefab.Transform.Position = new Vector3(0, 15, 15);
            lightPrefab.Light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            lightPrefab.AddComponent<DemoBehaviour>();

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = GraphicsHelper.CreateBorderTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128, 1);
            terrainMaterial.Shininess = 10;
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain");
            terrain.Flat();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            Screen.ShowCursor = true;
        }
    }
}
