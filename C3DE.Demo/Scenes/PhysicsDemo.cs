using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using C3DE.Utils;
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

            m_Camera.AddComponent<PhysicsSpawner>();

            SetControlMode(ControllerSwitcher.ControllerType.FPS, new Vector3(0, 2, 0), Vector3.Zero, true);

            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMaterial.NormalTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand_Normal");
            terrainMaterial.Shininess = 500;
            terrainMaterial.Tiling = new Vector2(32);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;
            Add(terrainGo);

            terrain.AddComponent<BoxCollider>();
            var rb = terrain.AddComponent<Rigidbody>();
            rb.IsStatic = true;
            rb.AddComponent<RigidbodyRenderer>();
            rb.AddComponent<BoundingBoxRenderer>().LineColor = Color.Red;
            rb.IsKinematic = true;

            m_Camera.AddComponent<Scripts.VRPlayerEnabler>();

            var content = Application.Content;
            var mat = new PBRMaterial
            {
                MainTexture = content.Load<Texture2D>("Textures/pbr/darktiles1_basecolor"),
                NormalMap = content.Load<Texture2D>("Textures/pbr/darktiles1_Normal-OGL"),
                AOMap = content.Load<Texture2D>("Textures/pbr/darktiles1_AO"),
            };

            mat.CreateRMSFromTextures(
                GraphicsHelper.CreateTexture(Color.White, 1, 1),
                GraphicsHelper.CreateTexture(Color.White, 1, 1),
                GraphicsHelper.CreateTexture(Color.White, 1, 1));

            var cube = GameObjectFactory.CreateMesh(GeometryType.Cube);
            cube.Transform.Translate(0, 3, -5);
            cube.Transform.LocalScale = new Vector3(3);
            var rc = cube.GetComponent<Renderer>();
            rc.Material = mat;
        }
    }
}
