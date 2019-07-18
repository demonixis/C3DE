using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
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
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);
            _directionalLight = lightGo.GetComponent<Light>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.StarsSkybox);

            var player = new PlayerShooter();
            Add(player);

            var groundMat = new StandardMaterial();
            groundMat.MainTexture = TextureFactory.CreateCheckboard(Color.White, Color.Black);
            groundMat.SpecularPower = 5;
            groundMat.Tiling = new Vector2(16);

            var ground = GameObjectFactory.CreateTerrain();
            var terrain = ground.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = groundMat;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;
            Add(ground);

            terrain.AddComponent<BoxCollider>();
            var rb = terrain.AddComponent<Rigidbody>();
            rb.IsStatic = true;
            rb.AddComponent<RigidbodyRenderer>();
            rb.AddComponent<BoundingBoxRenderer>().LineColor = Color.Red;
            rb.IsKinematic = true;

            for (var i = -50; i < 50; i += 10)
            {
                for (var j = -50; j < 50; j += 10)
                {
                    var cube = GameObjectFactory.CreateMesh(GeometryType.Cube);
                    cube.Transform.Position = new Vector3(i, 1, j);

                    var renderer = cube.GetComponent<MeshRenderer>();
                    renderer.Mesh.Size = new Vector3(2);
                    renderer.Mesh.Build();
                    renderer.Material = groundMat;

                    rb = cube.AddComponent<Rigidbody>();
                    rb.IsStatic = true;
                    rb.AddComponent<BoxCollider>();
                    rb.AddComponent<BoundingBoxRenderer>();
                }
            }
        }

        protected override void SceneSetup()
        {
        }
    }
}
