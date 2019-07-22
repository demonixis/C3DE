using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

            var mesh = GameObjectFactory.CreateMesh(GeometryType.Cube);
            mesh.GetComponent<MeshRenderer>().Material = new StandardMaterial
            {
                MainTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse")
            };

            // Terrain
            var terrainMaterial = new StandardWaterMaterial();
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse");
            terrainMaterial.Tiling = new Vector2(16);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;

            // Create the planar reflection.
            var planarGo = new GameObject("PlanarReflection");
            planarGo.Transform.Translate(0, 20, 0);
            var planar = planarGo.AddComponent<PlanarReflection>();
            planar.Initialize(Application.GraphicsDevice, 512);
            planar.AddComponent<ReflectionPlanarViewer>();

            // Assign to the water mesh.
            terrain.Renderer.PlanarReflection = planar;
        }
    }
}
