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
            m_Camera.AddComponent<VRPlayerEnabler>();
            Destroy(m_DirectionalLight.GetComponent<LensFlare>());

            SetControlMode(ControllerSwitcher.ControllerType.FPS, new Vector3(0, 2, 0), Vector3.Zero, true);

            // Setup the terrain.
            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = GraphicsHelper.CreateTexture(Color.White, 1, 1);
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

            var skyTextures = new[]
            {
                content.Load<Texture2D>("Textures/pbr/env/sky_right"),
                content.Load<Texture2D>("Textures/pbr/env/sky_left"),
                content.Load<Texture2D>("Textures/pbr/env/sky_top"),
                content.Load<Texture2D>("Textures/pbr/env/sky_bottom"),
                content.Load<Texture2D>("Textures/pbr/env/sky_forward"),
                content.Load<Texture2D>("Textures/pbr/env/sky_backward")
            };

            var irradianceTexture = GraphicsHelper.CreateCubeMap(new[]
            {
                content.Load<Texture2D>("Textures/pbr/env/irradiance_right"),
                content.Load<Texture2D>("Textures/pbr/env/irradiance_left"),
                content.Load<Texture2D>("Textures/pbr/env/irradiance_top"),
                content.Load<Texture2D>("Textures/pbr/env/irradiance_bottom"),
                content.Load<Texture2D>("Textures/pbr/env/irradiance_forward"),
                content.Load<Texture2D>("Textures/pbr/env/irradiance_backward")
            });

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, skyTextures, 256);

            // Generates the grid of spheres
            var startPos = -5f;
            var x = startPos;
            var z = startPos;
            var margin = 3.0f;

            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    var cube = GameObjectFactory.CreateMesh(GeometryType.Sphere);
                    cube.Transform.Translate(x, 1.5f, z);
                    cube.Transform.LocalScale = new Vector3(1);

                    var mat = new PBRMaterial
                    {
                        MainTexture = GraphicsHelper.CreateTexture(Color.White, 1, 1),
                        NormalMap = content.Load<Texture2D>("Textures/pbr/Metal01_nrm"),
                        AOMap = GraphicsHelper.CreateTexture(Color.White, 1, 1),
                        IrradianceMap = irradianceTexture
                    };

                    var r = (float)i / 5.0f;
                    var m = (float)j / 5.0f;

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
