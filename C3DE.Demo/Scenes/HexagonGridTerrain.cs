using C3DE.Components.Controllers;
using C3DE.Components.Lighting;
using C3DE.Demo.Scripts;
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
            var cameraGo = GameObjectFactory.CreateCamera();
            cameraGo.AddComponent<OrbitController>();
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);

            // Light
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional);
            lightGo.Transform.LocalRotation = new Vector3(-1, 1, 0);
            Add(lightGo);

            var grid = new GameObject("HexaGrid");
            grid.AddComponent<HexaGridBuilder>();
            Add(grid);

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            Screen.ShowCursor = true;
        }
    }
}
