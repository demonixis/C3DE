using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class PostProcessingDemo : Scene
    {
        public PostProcessingDemo() : base("Post Processing") { }

        public override void Initialize()
        {
            base.Initialize();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            // Camera
            var camera = new CameraPrefab("camera");
            camera.Transform.Translate(0, 25, 0);
            camera.AddComponent<DemoBehaviour>();
            camera.AddComponent<PostProcessSwitcher>();
            Add(camera);

            var orbitController = camera.AddComponent<OrbitController>();
            orbitController.KeyboardEnabled = false;

            // Light
            var lightPrefab = new LightPrefab("lightPrefab", LightType.Point);
            Add(lightPrefab);
            lightPrefab.Transform.Position = new Vector3(0, 15, 15);
            lightPrefab.Light.Range = 105;
            lightPrefab.Light.Intensity = 2.0f;
            lightPrefab.Light.FallOf = 5f;
            lightPrefab.Light.Color = Color.Violet;
            lightPrefab.Transform.Rotation = new Vector3(-1, 1, 0);
            lightPrefab.Light.Angle = 0.1f;
            lightPrefab.Light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;

            var ls = lightPrefab.AddComponent<LightSwitcher>();
            ls.SetBoxAlign(true);

            lightPrefab.AddComponent<LightMover>();
            lightPrefab.AddComponent<DemoBehaviour>();

            var lightPrefabSphere = lightPrefab.AddComponent<MeshRenderer>();
            lightPrefabSphere.Geometry = new SphereGeometry(2f, 4);
            lightPrefabSphere.Geometry.Build();
            lightPrefabSphere.CastShadow = false;
            lightPrefabSphere.ReceiveShadow = false;
            lightPrefabSphere.Material = new SimpleMaterial(scene);
            lightPrefabSphere.Material.Texture = GraphicsHelper.CreateTexture(Color.Yellow, 1, 1);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.Texture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMaterial.Shininess = 50;
            terrainMaterial.Tiling = new Vector2(8);

            var terrain = new TerrainPrefab("terrain");
            terrain.Renderer.Geometry.Size = new Vector3(2);
            terrain.Renderer.ReceiveShadow = true;
            terrain.Randomize(4, 15, 0.086, 0.25, true);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            // Lava
            var lava = new LavaPrefab("water");
            Add(lava);

            lava.Generate("Textures/lava_texture", "Textures/lava_bump", new Vector3(terrain.Width * 0.5f));

            var jack = new ModelPrefab("Jack");
            jack.Transform.Rotate(-MathHelper.PiOver2, 0, 0);
            jack.Transform.Translate(0, 35, 0);
            jack.Transform.LocalScale = new Vector3(4);
            jack.LoadModel("Models/Jack/JackOLantern");
            jack.Renderer.ReceiveShadow = true;
            jack.Renderer.CastShadow = true;
            var jackMaterial = new StandardMaterial(this);
            jackMaterial.EmissiveColor = new Color(0.2f, 0.005f, 0);
            jackMaterial.Texture = Application.Content.Load<Texture2D>("Models/Jack/PumpkinColor");
            jack.Renderer.Material = jackMaterial;
            Add(jack);

            orbitController.LookAt(jack.Transform);
            orbitController.Distance = 150.0f;
        }
    }
}
