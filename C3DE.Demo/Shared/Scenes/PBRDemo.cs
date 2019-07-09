using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class PBRDemo : SimpleDemo
    {
        public PBRDemo() : base("PBR") { }

        public override void Initialize()
        {
            base.Initialize();

            _camera.AddComponent<VRPlayerEnabler>();
          
            Destroy(_directionalLight.GetComponent<LensFlare>());

            SetControlMode(ControllerSwitcher.ControllerType.Orbit, new Vector3(0, 2, 0), Vector3.Zero, true);

            // Setup the terrain.
            var terrainMaterial = new PBRMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateCheckboard(Color.White, Color.Black);
            terrainMaterial.CreateRoughnessMetallicAO();
            terrainMaterial.Tiling = new Vector2(16);

            var go = GameObjectFactory.CreateTerrain();
            var terrain = go.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;
            terrain.Renderer.Enabled = true;
            Add(go);

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
            var renderAdditionalLights = true;

            if (Engine.Platform == GamePlatform.Android)
                renderAdditionalLights = false;

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

                    if (renderAdditionalLights)
                    {
                        lightGo = GameObjectFactory.CreateLight(LightType.Point, RandomHelper.GetColor(), 2, 0);
                        lightGo.Transform.Position = new Vector3(x, 5, z);
                        lightGo.AddComponent<SinMovement>();

                        light = lightGo.GetComponent<Light>();
                        light.ShadowGenerator.ShadowStrength = 1;

                        ligthSphere = lightGo.AddComponent<MeshRenderer>();
                        ligthSphere.Mesh = new SphereMesh(0.15f, 16);
                        ligthSphere.Mesh.Build();
                        ligthSphere.CastShadow = true;
                        ligthSphere.ReceiveShadow = false;
                        ligthSphere.Material = new UnlitMaterial() { DiffuseColor = lightGo.GetComponent<Light>().Color };
                    }

                    z += margin;
                }

                x += margin;
                z = -startPos;
            }
        }
    }
}
