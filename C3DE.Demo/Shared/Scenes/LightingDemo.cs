using C3DE.Components.Lighting;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Graphics;
using Microsoft.Xna.Framework.Content;
using C3DE.Components.Rendering;

namespace C3DE.Demo.Scenes
{
    public class LightingDemo : BaseDemo
    {
        public LightingDemo() : base("Realtime Lighting") { }

        public LightingDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            //Destroy(_directionalLight);
            //_directionalLight.Intensity = 0.05f;

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
            var mesh = model.ToMeshRenderers(PreferePBRMaterials);
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
            if (PreferePBRMaterials)
            {
                var groundMaterial = new PBRMaterial
                {
                    MainTexture = TextureFactory.CreateCheckboard(Color.White, Color.Black),
                    Tiling = new Vector2(16)
                };

                groundMaterial.CreateRoughnessMetallicAO();

                return groundMaterial;
            }

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
            if (PreferePBRMaterials)
            {
                var modelMaterial = new PBRMaterial()
                {
                    MainTexture = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse"),
                    NormalMap = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Normal"),
                    EmissiveMap = content.Load<Texture2D>("Models/Quandtum/textures/Turret-Emission"),
                };

                modelMaterial.CreateRoughnessMetallicAO(1.0f, 0.85f);

                return modelMaterial;
            }

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