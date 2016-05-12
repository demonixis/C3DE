using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Demo.Scripts;
using C3DE.Prefabs;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class HexagonTerrainDemo : Scene
    {
        public HexagonTerrainDemo() : base("Hexagonal Grid") { }

        public override void Initialize()
        {
            base.Initialize();

            // Camera
            var camera = new CameraPrefab("camera");
            camera.AddComponent<OrbitController>();
            camera.AddComponent<DemoBehaviour>();
            Add(camera);

            // Light
            var lightPrefab = new LightPrefab("lightPrefab", LightType.Directional);
            lightPrefab.Transform.Rotation = new Vector3(-1, 1, 0);
            Add(lightPrefab);

            var sceneObject = new GameObject("HexaGrid");
            sceneObject.AddComponent<HexaGridBuilder>();
            Add(sceneObject);

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            Screen.ShowCursor = true;
        }
    }
}
