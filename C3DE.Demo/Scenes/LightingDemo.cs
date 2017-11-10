using C3DE.Components;
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
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);

            // Light
            var padding = 50;
            var colors = new Color[] { Color.Yellow, Color.CornflowerBlue, Color.YellowGreen, Color.AntiqueWhite, Color.Cyan, Color.OrangeRed, Color.Purple, Color.Silver };
            var pos = new Vector3[]
            {
                new Vector3(padding, 16, padding),
                new Vector3(padding, 16, -padding),
                new Vector3(-padding, 16, padding),
                new Vector3(-padding, 16, -padding),
                new Vector3(0, 16, -padding * 2),
                new Vector3(0, 16, padding * 2),
                new Vector3(-padding * 2, 16, 0),
                new Vector3(padding * 2, 16, 0)
            };

            for (var i = 0; i < 8; i++)
            {
                var lightGo = GameObjectFactory.CreateLight(LightType.Point, colors[i], 1.5f, 1024);
                lightGo.Transform.Rotation = new Vector3(0.0f, 0.5f, 0);
                lightGo.Transform.Position = pos[i];
                Add(lightGo);
                
                var light = lightGo.GetComponent<Light>();
                light.Range = 60;
                light.ShadowGenerator.ShadowStrength = 1;
                light.ShadowGenerator.ShadowBias = 0.01f;

                var ligthSphere = lightGo.AddComponent<MeshRenderer>();
                ligthSphere.Geometry = new SphereGeometry(2f, 16);
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
            terrain.Renderer.CastShadow = false;
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

            var material = (StandardMaterial)renderer.Material;
            material.MainTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse");
            material.DiffuseColor = Color.Red;
            material.ReflectionMap = GraphicsHelper.CreateCubeMap(Application.Content.Load<Texture2D>("Textures/lava_texture"));
            material.ReflectionIntensity = 0.2f;
            renderer.Transform.LocalScale = new Vector3(0.1f);
            renderer.Transform.Rotate(0, -MathHelper.PiOver2, 0);
            renderer.Transform.Translate(-0.25f, 0, 0);
            Screen.ShowCursor = true;
        }
    }
}