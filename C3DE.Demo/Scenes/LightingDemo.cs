using C3DE.Components.Controllers;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Extensions;
using C3DE.Graphics.Geometries;
using C3DE.Graphics.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class LightingDemo : Scene
    {
        public LightingDemo() : base("Realtime Lighting") { }

        public override void Initialize()
        {
            base.Initialize();

            // Camera
            var cameraGo = GameObjectFactory.CreateCamera();
            var orbit = cameraGo.AddComponent<OrbitController>();
            orbit.KeyboardEnabled = false;
            orbit.MaxDistance = 2000;
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);

            // Light
            var padding = 15;
            var colors = new Color[] { Color.Red, Color.CornflowerBlue, Color.YellowGreen, Color.AntiqueWhite, Color.Cyan, Color.OrangeRed, Color.Purple, Color.Silver };
            var pos = new Vector3[]
            {
                new Vector3(padding, 10, padding),
                new Vector3(padding, 10, -padding),
                new Vector3(-padding, 10, padding),
                new Vector3(-padding, 10, -padding),
                new Vector3(0, 10, -padding * 2),
                new Vector3(0, 10, padding * 2),
                new Vector3(-padding * 2, 10, 0),
                new Vector3(padding * 2, 10, 0)
            };

            for (var i = 0; i < 8; i++)
            {
                var lightGo = GameObjectFactory.CreateLight(LightType.Point, colors[i], 1f, i == 0 ? 1024 : 0);
                lightGo.Transform.Position = pos[i];
                Add(lightGo);

                var light = lightGo.GetComponent<Light>();
                light.Range = 10;

                var ligthSphere = lightGo.AddComponent<MeshRenderer>();
                ligthSphere.Geometry = new SphereGeometry(2f, 4);
                ligthSphere.Geometry.Build();
                ligthSphere.CastShadow = true;
                ligthSphere.ReceiveShadow = false;
                ligthSphere.Material = new UnlitColorMaterial(scene);
                ligthSphere.Material.DiffuseColor = colors[i];
                ligthSphere.AddComponent<LightMover>();
                ligthSphere.AddComponent<LightSwitcher>();

                if (i == 0)
                    ligthSphere.AddComponent<ShadowMapViewer>();
            }

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = GraphicsHelper.CreateBorderTexture(Color.CornflowerBlue, Color.Black, 128, 128, 2);
            terrainMaterial.Shininess = 150;
            terrainMaterial.Tiling = new Vector2(32);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(4);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = true;
            terrainGo.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrainGo);

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // Model
            var model = Application.Content.Load<Model>("Models/Quandtum/Quandtum");
            var mesh = model.ToMeshRenderers(this);
            var renderer = mesh.GetComponentInChildren<MeshRenderer>();
            renderer.CastShadow = true;
            renderer.ReceiveShadow = true;
            renderer.Material.MainTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse");
            renderer.Transform.LocalScale = new Vector3(0.1f);
            renderer.Transform.Rotate(0, -MathHelper.PiOver2, 0);
            renderer.Transform.Translate(-0.25f, 0, 0);
            Screen.ShowCursor = true;
        }
    }
}