using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class TestDemo : Scene
    {
        public TestDemo() : base("Test")
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            GameObjectFactory.CreateLight(C3DE.Components.Lighting.LightType.Directional);

            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            var _camera = camera.GetComponent<Camera>();
            _camera.AddComponent<DemoBehaviour>();
            var _controllerSwitcher = _camera.AddComponent<ControllerSwitcher>();

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.BlueSkybox, 2048);

            // Terrain
            var terrainMaterial = new StandardWaterMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateCheckboard(Color.White, Color.Black);
            terrainMaterial.Tiling = new Vector2(16);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(4);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;
        }
    }
}
