using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes.PBR
{
    public class SimplePBRDemo : SimpleDemo
    {
        public SimplePBRDemo() : base("Simple PBR") { }

        public static Texture2D CreateRMAOFromBlackBoard(Texture2D albedo)
        {
            var width = albedo.Width;
            var height = albedo.Height;
            var size = width * height;

            var albedoColors = new Color[size];
            albedo.GetData<Color>(albedoColors);

            var rmaoColors = new Color[size];

            for(var i = 0; i < size; i++)
            {
                rmaoColors[i].R = albedoColors[i].R;
                rmaoColors[i].R = 0;
                rmaoColors[i].R = 255;
            }

            var rmao = new Texture2D(Application.GraphicsDevice, width, height);
            rmao.SetData<Color>(rmaoColors);

            return rmao;
        }

        public override void Initialize()
        {
            base.Initialize();

            _camera.AddComponent<VRPlayerEnabler>();
          
            Destroy(_directionalLight.GetComponent<LensFlare>());

            SetControlMode(ControllerSwitcher.ControllerType.Orbit, new Vector3(0, 2, 0), Vector3.Zero, true);

            // Setup the terrain.
            var terrainMaterial = new PBRMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateCheckboard(Color.White, Color.Black);
            terrainMaterial.RoughnessMetalicAOMap = CreateRMAOFromBlackBoard(terrainMaterial.MainTexture);

            var go = GameObjectFactory.CreateTerrain();
            var terrain = go.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.TextureRepeat = new Vector2(16);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;
            terrain.Renderer.Enabled = true;

            // Generate the Sky
            var content = Application.Content;

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.NatureSkybox, 256);
            RenderSettings.FogMode = FogMode.None;

            // Caches
            GameObject cube = null;
            GameObject lightGo = null;
            Light light;
            PBRMaterial pbrMaterial = null;
            Renderer renderer = null;
            MeshRenderer ligthSphere = null;

            // Generates the grid of spheres
            var startPos = 7.0f;
            var x = -startPos;
            var z = -startPos;
            var margin = 2.5f;

            for (var i = 0; i < (int)startPos; i++)
            {
                for (var j = 0; j < (int)startPos; j++)
                {
                    cube = GameObjectFactory.CreateMesh(GeometryType.Sphere);
                    cube.Transform.Translate(x, 1.5f, z);

                    pbrMaterial = new PBRMaterial
                    {
                        MainTexture = TextureFactory.CreateColor(Color.White, 1, 1),
                    };

                    var roughness = (float)i / startPos;
                    var metallic = (float)j / startPos;

                    pbrMaterial.CreateRoughnessMetallicAO(roughness, metallic);

                    renderer = cube.GetComponent<Renderer>();
                    renderer.Material = pbrMaterial;
                    renderer.CastShadow = true;
                    renderer.ReceiveShadow = true;

                    z += margin;
                }

                x += margin;
                z = -startPos;
            }

            // Light
            SpawnRadialLights(1f, 0.0f, 8);
            SpawnRadialLights(5, 0.0f, 8);
            SpawnRadialLights(10, 0.0f, 8);
        }
    }
}
