using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
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

            SetControlMode(ControllerSwitcher.ControllerType.FPS, new Vector3(0, 2, 0), Vector3.Zero, true);

            // Setup the terrain.
            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateColor(Color.White, 1, 1);
            terrainMaterial.Shininess = 32;
            terrainMaterial.Tiling = new Vector2(16);

            var go = GameObjectFactory.CreateTerrain();
            var terrain = go.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;
            terrain.Renderer.Enabled = false;
            Add(go);

            // Generate the Sky
            var content = Application.Content;

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.NatureSkybox, 256);

            RenderSettings.FogMode = FogMode.None;

            // Caches
            GameObject cube = null;
            GameObject light = null;
            PBRMaterial pbrMaterial = null;
            Renderer renderer = null;

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

                    pbrMaterial.CreateRMSFromValues(roughness, metallic);

                    renderer = cube.GetComponent<Renderer>();
                    renderer.Material = pbrMaterial;

                    if (renderAdditionalLights)
                    {
                        light = GameObjectFactory.CreateLight(LightType.Point, RandomHelper.GetColor(), 2, 0);
                        light.Transform.Position = new Vector3(x, 5, z); 
                    }

                    z += margin;
                }

                x += margin;
                z = -startPos;
            }
        }
    }
}
