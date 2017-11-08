using C3DE.Components.Controllers;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Extensions;
using C3DE.Graphics.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class TestDemo : Scene
    {
        public TestDemo() : base("Test") { }

        public override void Initialize()
        {
            base.Initialize();

            // Camera
            var cameraGo = GameObjectFactory.CreateCamera();
            var orbit = cameraGo.AddComponent<OrbitController>();
            orbit.KeyboardEnabled = false;
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);

            // Light
            var lightGo = GameObjectFactory.CreateLight(LightType.Point, Color.GhostWhite, 1, 0);
            lightGo.Transform.Position = new Vector3(0, 15, 15);
            lightGo.Transform.Rotation = new Vector3(-1, 1, 0);
            lightGo.AddComponent<LightSwitcher>();
            lightGo.AddComponent<LightMover>();
            Add(lightGo);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.Texture = GraphicsHelper.CreateBorderTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128, 4);
            terrainMaterial.Shininess = 10;
            terrainMaterial.Tiling = new Vector2(16);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(4);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrainGo.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrainGo);

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // Model
            var model = Application.Content.Load<Model>("Models/Quandtum/Quandtum");
            var mesh = model.ToMeshRenderers(this);
            var renderer = mesh.GetComponentInChildren<MeshRenderer>();
            renderer.Material.Texture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse");
            renderer.Transform.LocalScale = new Vector3(0.1f);
            renderer.Transform.Rotate(0, -MathHelper.PiOver2, 0);
            renderer.Transform.Translate(-0.25f, 0, 0);
            Screen.ShowCursor = true;
        }
    }
}
