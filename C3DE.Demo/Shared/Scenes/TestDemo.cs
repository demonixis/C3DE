using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Components.Lighting;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using C3DE.Graphics;

namespace C3DE.Demo.Scenes
{
    public class TestDemo : BaseDemo
    {
        public TestDemo() : base("Test")
        {
        }

        protected override void SceneSetup()
        {
            // Keep that empty to not setup the default scene.
        }

        public override void Initialize()
        {
            base.Initialize();

            var lightGo = GameObjectFactory.CreateLight(LightType.Point, Color.Red, 10f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(0, 10, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);

            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            var _camera = camera.GetComponent<Camera>();
            _camera.AddComponent<DemoBehaviour>();
            var _controllerSwitcher = _camera.AddComponent<ControllerSwitcher>();

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.BlueSkybox, 256);

            var content = Application.Content;
            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateCheckboard(Color.Black, Color.White);
            terrainMaterial.Tiling = new Vector2(16);
            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;
        }
    }
}
