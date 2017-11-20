using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class PhysicsDemo : Scene
    {
        public PhysicsDemo() : base("Physics") { }

        public override void Initialize()
        {
            base.Initialize();

            var cameraGo = GameObjectFactory.CreateCamera();
            cameraGo.AddComponent<ControllerSwitcher>();
            cameraGo.AddComponent<PhysicsSpawner>();
            Add(cameraGo);

            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.LightSteelBlue, 1.5f);
            lightGo.Transform.LocalPosition = new Vector3(-20, 20, 0);
            lightGo.Transform.LocalRotation = new Vector3(1, -1, 0);
            lightGo.AddComponent<DemoBehaviour>();
            Add(lightGo);

            lightGo.GetComponent<Light>().Range = 100;

            var terrainMaterial = new StandardMaterial(scene);
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
            terrainGo.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrainGo);

            terrain.AddComponent<BoxCollider>();
            var rb = terrain.AddComponent<Rigidbody>();
            rb.IsStatic = true;
            rb.AddComponent<RigidbodyRenderer>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            Screen.ShowCursor = true;
        }
    }
}
