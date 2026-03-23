using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Graphics;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Demo.Scenes
{
    public class LightingDemo : BaseDemo
    {
        public LightingDemo() : base("Realtime Lighting") { }

        public LightingDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            RenderSettings.Skybox.GenerateProcedural(1800.0f);
            RenderSettings.Skybox.ProceduralSettings.AutoCycle = true;
            RenderSettings.Skybox.ProceduralSettings.TimeOfDay = 0.28f;
            RenderSettings.Skybox.ProceduralSettings.CycleSpeed = 0.015f;
            RenderSettings.Skybox.ProceduralSettings.CloudCoverage = 0.5f;
            RenderSettings.Skybox.ProceduralSettings.StarIntensity = 1.2f;

            var skyController = new GameObject("ProceduralSkyController");
            skyController.AddComponent<ProceduralSkyController>();
            _camera.AddComponent<ProceduralSkySwitcher>();

            // Reflection Probe
            var probe = GameObjectFactory.CreateReflectionProbe(new Vector3(0, 35, 0), 32, 60, 900, 1000);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(4);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = GetGroundMaterial(probe.ReflectionMap);
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;

            var content = Application.Content;

            // Model
            var model = content.Load<Model>("Models/Quandtum/Quandtum");
            var mesh = model.ToMeshRenderers();
            mesh.Transform.LocalScale = new Vector3(0.25f);
            mesh.Transform.Rotate(0, 0, -MathHelper.PiOver2);

            var renderer = mesh.GetComponentInChildren<MeshRenderer>();
            renderer.CastShadow = true;
            renderer.ReceiveShadow = true;
            renderer.Transform.LocalScale = new Vector3(0.035f);
            renderer.Transform.Rotate(0, -MathHelper.PiOver2, 0);
            renderer.Transform.Translate(-0.1f, 0, 0);
            renderer.Material = GetModelMaterial(content, probe.ReflectionMap);

            // Light
            AddLightGroundTest();
        }

        private Material GetGroundMaterial(TextureCube reflectionMap)
        {
            return new StandardMaterial
            {
                MainTexture = TextureFactory.CreateCheckboard(Color.White, Color.Black),
                SpecularPower = 2,
                Tiling = new Vector2(32),
                ReflectionIntensity = 0.45f,
                ReflectionMap = reflectionMap,
            };
        }

        private Material GetModelMaterial(ContentManager content, TextureCube reflectionMap)
        {
            return new StandardMaterial
            {
                MainTexture = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse"),
                NormalMap = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Normal"),
                EmissiveMap = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Emission"),
                SpecularMap = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Specular"),
                SpecularPower = 8,
                SpecularColor = Color.White,
                EmissiveColor = Color.White,
                EmissiveIntensity = 1,
                ReflectionMap = reflectionMap,
                ReflectionIntensity = 0.65f
            };
        }
    }
}
