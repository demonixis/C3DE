using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Components.Lighting;
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

            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);

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
            var terrainMaterial = new StandardTerrainMaterial();
            terrainMaterial.MainTexture = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_col");
            terrainMaterial.GrassNormalMap = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_nrm");
            terrainMaterial.SandTexture = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_col");
            terrainMaterial.SandNormalMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_nrm");
            terrainMaterial.SnowTexture = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_col");
            terrainMaterial.SnownNormalMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_nrm");
            terrainMaterial.RockTexture = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_col");
            terrainMaterial.RockNormalMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_nrm");

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Build();
            terrain.Randomize();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;

            SimpleDemo.AddLightGroundTest();
        }
    }
}
