using C3DE.Components.Controllers;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Materials;
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
            var cameraGo = GameObjectFactory.CreateCamera();
            cameraGo.AddComponent<DemoBehaviour>();
            cameraGo.AddComponent<PostProcessSwitcher>();
            Add(cameraGo);

            var orbitController = cameraGo.AddComponent<OrbitController>();
            orbitController.KeyboardEnabled = false;

            // Light
            var lightGo = GameObjectFactory.CreateLight(LightType.Point, Color.White, 1.0f, 1024);
            lightGo.AddComponent<LightMover>();
            lightGo.AddComponent<DemoBehaviour>();
            lightGo.Transform.LocalPosition = new Vector3(50, 50, 50);
            lightGo.Transform.LocalRotation = new Vector3(0.0f, 0.5f, 0);
            Add(lightGo);

            var ls = lightGo.AddComponent<LightSwitcher>();
            ls.SetBoxAlign(true);

            var ligthSphere = lightGo.AddComponent<MeshRenderer>();
            ligthSphere.Geometry = new SphereMesh(2f, 16);
            ligthSphere.Geometry.Build();
            ligthSphere.CastShadow = false;
            ligthSphere.ReceiveShadow = false;
            ligthSphere.Material = new SimpleMaterial(scene);
            ligthSphere.Material.MainTexture = GraphicsHelper.CreateTexture(Color.Yellow, 1, 1);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMaterial.Shininess = 250;
            terrainMaterial.Tiling = new Vector2(8);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Renderer.Geometry.Size = new Vector3(2);
            terrain.Renderer.ReceiveShadow = true;
            terrain.Randomize(4, 15, 0.086, 0.25, true);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrainGo);

            // Lava
            var lavaTexture = Application.Content.Load<Texture2D>("Textures/lava_texture");
            var lavaNormal = Application.Content.Load<Texture2D>("Textures/lava_bump");
            var lava = GameObjectFactory.CreateLava(lavaTexture, lavaNormal, new Vector3(terrain.Width * 0.5f));
            Add(lava);

            var jackModel = Application.Content.Load<Model>("Models/Jack/JackOLantern");
            var jackOLenternGo = GameObjectFactory.CreateXNAModel(jackModel);
            jackOLenternGo.Transform.Rotate(-MathHelper.PiOver2, 0, 0);
            jackOLenternGo.Transform.Translate(0, 35, 0);
            jackOLenternGo.Transform.LocalScale = new Vector3(3);

            var jackRenderer = jackOLenternGo.GetComponent<ModelRenderer>();
            jackRenderer.ReceiveShadow = true;
            jackRenderer.CastShadow = true;

            var jackMaterial = new StandardMaterial(this);
            jackMaterial.DiffuseColor = new Color(0.2f, 0.005f, 0);
            jackMaterial.EmissiveColor = new Color(0.2f, 0.005f, 0);
            jackMaterial.EmissiveEnabled = true;
            jackMaterial.EmissiveIntensity = 1.25f;
            //jackMaterial.EmissiveTexture = Application.Content.Load<Texture2D>("Models/Jack/PumpkinColor");
            //jackMaterial.MainTexture = Application.Content.Load<Texture2D>("Models/Jack/PumpkinColor");
            jackRenderer.Material = jackMaterial;
            Add(jackOLenternGo);

            orbitController.LookAt(jackOLenternGo.Transform);
            orbitController.Distance = 150.0f;
            //orbitController.AddComponent<EmissiveViewer>();
        }
    }
}
