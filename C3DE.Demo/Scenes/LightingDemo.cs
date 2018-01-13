using C3DE.Components.Controllers;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class LightingDemo : SimpleDemo
    {
        public LightingDemo() : base("Realtime Lighting") { }

        public LightingDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            Destroy(m_DirectionalLight);

            // Light
            var padding = 20;
            var colors = new Color[] {
                Color.Red,
                Color.Green,
                Color.Blue,
                Color.Yellow,
                Color.Pink,
                Color.Cyan,
                Color.Purple,
                Color.Olive
            };

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
                var lightGo = GameObjectFactory.CreateLight(LightType.Point, colors[i], 0.5f, 1024);
                lightGo.Transform.LocalRotation = new Vector3(0.0f, 0.5f, 0);
                lightGo.Transform.LocalPosition = pos[i];
                Add(lightGo);
                
                var light = lightGo.GetComponent<Light>();
                light.Range = 25;
                light.ShadowGenerator.ShadowStrength = 1;
                light.ShadowGenerator.ShadowBias = 0.01f;

                var ligthSphere = lightGo.AddComponent<MeshRenderer>();
                ligthSphere.Geometry = new SphereMesh(2f, 16);
                ligthSphere.Geometry.Build();
                ligthSphere.CastShadow = true;
                ligthSphere.ReceiveShadow = false;

                var sphereMaterial = new StandardMaterial();
                sphereMaterial.DiffuseColor = colors[i];
                sphereMaterial.EmissiveIntensity = 1;
                sphereMaterial.EmissiveColor = colors[i];
                sphereMaterial.EmissiveEnabled = true;
                ligthSphere.Material = sphereMaterial;
                
                ligthSphere.AddComponent<LightMover>();
                ligthSphere.AddComponent<LightSwitcher>();
                ligthSphere.AddComponent<SinMovement>();
            }
            
            // Terrain
            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMaterial.NormalTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand_Normal");
            terrainMaterial.Shininess = 500;
            terrainMaterial.Tiling = new Vector2(32);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(4);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;
            Add(terrainGo);

            // Model
            var model = Application.Content.Load<Model>("Models/Quandtum/Quandtum");
            var mesh = model.ToMeshRenderers(this);
            var renderer = mesh.GetComponentInChildren<MeshRenderer>();
            renderer.CastShadow = true;
            renderer.ReceiveShadow = true;

            var material = (StandardMaterial)renderer.Material;
            material.MainTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse");
            material.SpecularTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Specular");
            material.NormalTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Normal");
            material.EmissiveTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Emission");
            material.EmissiveEnabled = true;
            material.EmissiveIntensity = 2.0f;
            material.EmissiveColor = Color.Red;
            material.ReflectionTexture = RenderSettings.Skybox.Texture;
            material.Shininess = 250;
            renderer.Transform.LocalScale = new Vector3(0.035f);
            renderer.Transform.Rotate(0, -MathHelper.PiOver2, 0);
            renderer.Transform.Translate(-0.1f, 0, 0);

            m_Camera.AddComponent<Scripts.VRPlayerEnabler>();
        }
    }
}