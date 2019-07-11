using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using C3DE.Utils;
using C3DE.Graphics;

namespace C3DE.Demo.Scenes.PBR
{
    public class LightingPBRDemo : SimpleDemo
    {
        public LightingPBRDemo() : base("Realtime PBR Lighting") { }

        public LightingPBRDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            _camera.AddComponent<VRPlayerEnabler>();

            SetControlMode(ControllerSwitcher.ControllerType.FPS, Vector3.Zero, Vector3.Zero);

            Destroy(_directionalLight.GetComponent<LensFlare>());

            // Light
            SpawnLights(1, 0.0f, 8);
            SpawnLights(4, 0.0f, 16);
            SpawnLights(7, 0.0f, 16);

            // Setup the terrain.
            var terrainMaterial = new PBRMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateCheckboard(Color.White, Color.Black);
            terrainMaterial.CreateRoughnessMetallicAO();
            terrainMaterial.Tiling = new Vector2(16);

            var go = GameObjectFactory.CreateTerrain();
            var terrain = go.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;
            terrain.Renderer.Enabled = true;
            Add(go);

            var content = Application.Content;

            // Model
            var model = Application.Content.Load<Model>("Models/Quandtum/Quandtum");
            var mesh = model.ToMeshRenderers(this);
            mesh.Transform.LocalScale = new Vector3(0.15f);
            mesh.Transform.Rotate(0, 0, -MathHelper.PiOver2);

            var renderer = mesh.GetComponentInChildren<MeshRenderer>();
            renderer.CastShadow = true;
            renderer.ReceiveShadow = true;
            renderer.Transform.LocalScale = new Vector3(0.035f);
            renderer.Transform.Rotate(0, -MathHelper.PiOver2, 0);
            renderer.Transform.Translate(-0.1f, 0, 0);

            var modelMaterial = new PBRMaterial()
            {
                MainTexture = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse"),
                NormalMap = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Normal"),
                EmissiveMap = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Emission"),
            };

            modelMaterial.CreateRoughnessMetallicAO(1.0f, 0.85f);

            renderer.Material = modelMaterial;

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.StarsSkybox, 256);
            RenderSettings.FogMode = FogMode.None;
        }

        private void SpawnLights(float radius, float y, int spawnCount)
        {
            var colors = new []
            {
                Color.Red, Color.Green, Color.Blue,
                Color.Purple, Color.Cyan, Color.Yellow
            };

            Color color;
            GameObject lightGo;
            Light light;
            MeshRenderer ligthSphere;

            for (var i = 0; i < spawnCount; i++)
            {
                var angle = i * MathHelper.TwoPi / 8.0f;

                color = colors[RandomHelper.Range(0, colors.Length)];

                lightGo = GameObjectFactory.CreateLight(LightType.Point, color, 1.0f, 0);
                lightGo.Transform.LocalRotation = new Vector3(0.0f, 0.5f, 0);
                lightGo.Transform.LocalPosition = new Vector3((float)Math.Cos(angle) * radius, y, (float)Math.Sin(angle) * radius);

                light = lightGo.GetComponent<Light>();
                light.Radius = 5;
                light.ShadowEnabled = false;

                ligthSphere = lightGo.AddComponent<MeshRenderer>();
                ligthSphere.Mesh = new SphereMesh(0.15f, 16);
                ligthSphere.Mesh.Build();
                ligthSphere.CastShadow = true;
                ligthSphere.ReceiveShadow = false;

                ligthSphere.Material = new UnlitMaterial()
                {
                    DiffuseColor = color
                };

                ligthSphere.AddComponent<LightMover>();
                ligthSphere.AddComponent<LightSwitcher>();
                ligthSphere.AddComponent<SinMovement>();
            }
        }
    }
}