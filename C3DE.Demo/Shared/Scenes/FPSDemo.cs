using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class FPSDemo : SimpleDemo
    {
        public static bool Instancing = false;
        public static bool PreferePBR = false;

        public FPSDemo() : base("First Person Shooter") { }

        public override void Initialize()
        {
            base.Initialize();

            // And a light
            var lightContainer = new GameObject("LightContainer");
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 0.75f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);
            lightGo.Transform.Parent = lightContainer.Transform;

            var autoRotation = lightContainer.AddComponent<AutoRotation>();
            autoRotation.Rotation = new Vector3(0, 0.25f, 0);

            _directionalLight = lightGo.GetComponent<Light>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.StarsSkybox, 2000);

            var player = new PlayerShooter();
            Add(player);

            var content = Application.Content;

            // Ground
            var ground = GameObjectFactory.CreateTerrain();
            var terrain = ground.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = CreateGroundMaterial(content);
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;

            terrain.AddComponent<BoxCollider>();
            var rb = terrain.AddComponent<Rigidbody>();
            rb.IsStatic = true;
            rb.AddComponent<RigidbodyRenderer>();
            rb.AddComponent<BoundingBoxRenderer>().LineColor = Color.Red;
            rb.IsKinematic = true;

            // Walls
            int cubeSize = 4;
            MeshRenderer instancedRenderer = null;
            CubeMesh cubeMesh = new CubeMesh();
            cubeMesh.Size = new Vector3(cubeSize);
            cubeMesh.Build();
            var wallMaterial = CreateWallMaterial(content);

            var counter = 0;

            for (var i = -50; i < 50; i += 10)
            {
                for (var j = -50; j < 50; j += 10)
                {
                    var cube = GameObjectFactory.CreateMesh(GeometryType.Cube);
                    cube.Transform.Position = new Vector3(i, (float)cubeSize / 2.0f, j);

                    var renderer = cube.GetComponent<MeshRenderer>();
                    renderer.Mesh = cubeMesh;
                    renderer.ComputeBoundingInfos();
                    renderer.Material = wallMaterial;

                    rb = cube.AddComponent<Rigidbody>();
                    rb.IsStatic = true;
                    rb.AddComponent<BoxCollider>();
                    rb.AddComponent<BoundingBoxRenderer>();

                    if (counter++ % 2 == 0)
                    {
                        var light = SpawnLight(new Vector3(i - 5, 3, j - 5), ValidColors[RandomHelper.Range(0, ValidColors.Length)], 10, 1, false);
                        light.AddComponent<SinIntensity>();
                    }

                    if (Instancing)
                    {
                        if (i == -50 && j == -50)
                        {
                            instancedRenderer = renderer;
                        }
                        else
                        {
                            instancedRenderer.AddInstance(renderer);
                            renderer.Enabled = false;
                        }
                    }
                }
            }

            // Planets
            var planetContainer = new GameObject("PlanetContainer");
            autoRotation = planetContainer.AddComponent<AutoRotation>();
            autoRotation.Rotation = new Vector3(0, 0.05f, 0);

            var material = new StandardMaterial();
            material.MainTexture = content.Load<Texture2D>($"Textures/Planets/2k_mercury");
            material.NormalMap = content.Load<Texture2D>($"Textures/Planets/2k_mercury_normal");

            var planet = GameObjectFactory.CreateMesh(GeometryType.Sphere, true, true, false);
            planet.GetComponent<Renderer>().Material = material;
            planet.Transform.LocalPosition = new Vector3(150, 60, 100);
            planet.Transform.LocalScale = new Vector3(60);
            planet.Transform.Parent = planetContainer.Transform;

            autoRotation = planet.AddComponent<AutoRotation>();
            autoRotation.Rotation = new Vector3(0, -0.1f, 0);

            // Bloom
            var bloom = new FastBloom(Application.GraphicsDevice);
            bloom.blurIterations = 4;
            bloom.blurType = FastBloom.BlurType.Sgx;
            bloom.resolution = FastBloom.Resolution.High;
            SetPostProcess(bloom, true);
        }

        protected override void SceneSetup()
        {
        }

        private Material CreateWallMaterial(ContentManager content)
        {
            if (PreferePBR)
            {
                var wallMatPBR = new PBRMaterial()
                {
                    MainTexture = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_basecolor"),
                    NormalMap = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_normal"),
                    //SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                    //SpecularPower = 5
                };

                wallMatPBR.CreateRoughnessMetallicAO(
                    content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_metallic"),
                    content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_roughness"),
                    content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_ambientOcclusion"));

                return wallMatPBR;
            }

            return new StandardMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_basecolor"),
                NormalMap = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_normal"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 5
            };
        }

        private Material CreateGroundMaterial(ContentManager content)
        {
            if (PreferePBR)
            {
                var mat = new PBRMaterial()
                {
                    MainTexture = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_basecolor"),
                    NormalMap = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_normal"),
                    Tiling = new Vector2(16)
                };

                mat.CreateRoughnessMetallicAO(
                    content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_roughness"),
                    content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_metallic"),
                    content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_ambientOcclusion"));

                return mat;
            }

            return new StandardMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_basecolor"),
                NormalMap = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_normal"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 10,
                ReflectionIntensity = 0.75f,
                Tiling = new Vector2(16)
            };
        }
    }
}
