using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using C3DE.Utils;
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
            terrainMaterial.MainTexture = GraphicsHelper.CreateTexture(Color.White, 1, 1);
            terrainMaterial.Shininess = 32;
            terrainMaterial.Tiling = new Vector2(16);

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

            var startPos = -5f;
            var x = startPos;
            var z = startPos;
            var margin = 5.0f;

            for(var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var cube = GameObjectFactory.CreateMesh(GeometryType.Sphere);
                    cube.Transform.Translate(x, 3.0f, z);
                    cube.Transform.LocalScale = new Vector3(2);

                    var mat = new PBRMaterial
                    {
                        MainTexture = GraphicsHelper.CreateTexture(Color.White, 1, 1),
                        NormalMap = content.Load<Texture2D>("Textures/pbr/Metal01_nrm"),
                        AOMap = GraphicsHelper.CreateTexture(Color.White, 1, 1)
                    };

                    var r = (float)i / 10.0f;
                    var m = (float)j / 10.0f;

                    mat.CreateRMSFromTextures(
                        GraphicsHelper.CreateTexture(new Color(r, r, r), 1, 1),
                        GraphicsHelper.CreateTexture(new Color(m, m, m), 1, 1),
                        null);

                    var rc = cube.GetComponent<Renderer>();
                    rc.Material = mat;

                    z += margin;
                }
                x += margin;
                z = startPos;
            }
        }
    }
}
