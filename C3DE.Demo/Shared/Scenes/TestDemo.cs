using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Components.Lighting;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Graphics.Rendering;

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

            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);

            Application.Engine.Renderer = new DeferredRenderer(Application.GraphicsDevice);
            lightGo.AddComponent<DeferredDebuger>();

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
            /*var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse");
            terrainMaterial.Tiling = new Vector2(16);*/

            var content = Application.Content;
            var terrainMaterial = new UnlitMaterial();
            terrainMaterial.DiffuseColor = Color.Red;
            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Build();
            terrain.Randomize();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;
        }
    }
}
