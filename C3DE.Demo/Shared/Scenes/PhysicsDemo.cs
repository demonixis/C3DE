using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class PhysicsDemo : SimpleDemo
    {
        public PhysicsDemo() : base("Physics") { }

        public override void Initialize()
        {
            base.Initialize();

            // And a light
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1.5f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);

            var autoRotation = lightGo.AddComponent<AutoRotation>();
            autoRotation.Rotation = new Vector3(0, 1, 0);

            _directionalLight = lightGo.GetComponent<Light>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.StarsSkybox);

            var player = new PlayerShooter();
            Add(player);

            var content = Application.Content;

            var groundMat = new StandardMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_basecolor"),
                NormalMap = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_normal"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 10,
                Tiling = new Vector2(16)
            };

            var ground = GameObjectFactory.CreateTerrain();
            var terrain = ground.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = groundMat;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;

            terrain.AddComponent<BoxCollider>();
            var rb = terrain.AddComponent<Rigidbody>();
            rb.IsStatic = true;
            rb.AddComponent<RigidbodyRenderer>();
            rb.AddComponent<BoundingBoxRenderer>().LineColor = Color.Red;
            rb.IsKinematic = true;

            var wallMat = new StandardMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_basecolor"),
                NormalMap = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_normal"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 5
            };

            MeshRenderer instancedRenderer = null;
            CubeMesh cubeMesh = new CubeMesh();
            cubeMesh.Size = new Vector3(2);
            cubeMesh.Build();

            for (var i = -50; i < 50; i += 10)
            {
                for (var j = -50; j < 50; j += 10)
                {
                    var cube = GameObjectFactory.CreateMesh(GeometryType.Cube);
                    cube.Transform.Position = new Vector3(i, 1, j);

                    var renderer = cube.GetComponent<MeshRenderer>();
                    renderer.Mesh = cubeMesh;
                    renderer.Material = wallMat;

                    rb = cube.AddComponent<Rigidbody>();
                    rb.IsStatic = true;
                    rb.AddComponent<BoxCollider>();
                    rb.AddComponent<BoundingBoxRenderer>();

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

            AddLightGroundTest();
        }

        protected override void SceneSetup()
        {
        }
    }
}
