using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class PBRDemo : SimpleDemo
    {
        public PBRDemo() : base("PBR") { }

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

            m_Camera.AddComponent<VRPlayerEnabler>();

            var content = Application.Content;
            var mat = new PBRMaterial
            {
                MainTexture = content.Load<Texture2D>("Textures/pbr/cerberus_A"),
                NormalMap = content.Load<Texture2D>("Textures/pbr/cerberus_N"),
                AOMap = content.Load<Texture2D>("Textures/pbr/cerberus_AO")
            };

            mat.CreateRMSFromTextures(
                content.Load<Texture2D>("Textures/pbr/cerberus_R"),
                content.Load<Texture2D>("Textures/pbr/cerberus_M"),
                content.Load<Texture2D>("Textures/pbr/cerberus_S"));

            for (var i = -50; i < 50; i += 10)
            {
                for (var j = -50; j < 50; j += 10)
                {
                    var cube = GameObjectFactory.CreateMesh(GeometryType.Sphere);
                    cube.Transform.Translate(i, 3, j);
                    cube.Transform.LocalScale = new Vector3(3);

                    var rc = cube.GetComponent<Renderer>();
                    rc.Material = mat;
                }
            } 
        }
    }
}
