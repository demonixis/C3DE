using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Graphics;

namespace C3DE.Demo.Scenes
{
    public class LightingDemo : SimpleDemo
    {
        public LightingDemo() : base("Realtime Lighting") { }

        public LightingDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            //Destroy(_directionalLight);
            _directionalLight.Intensity = 0.05f;

            // Terrain
            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateCheckboard(Color.White, Color.Black);
            terrainMaterial.SpecularPower = 2;
            terrainMaterial.ReflectionIntensity = 0.85f;
            terrainMaterial.Tiling = new Vector2(32);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(4);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;

            // Model
            var model = Application.Content.Load<Model>("Models/Quandtum/Quandtum");
            var mesh = model.ToMeshRenderers(this);
            mesh.Transform.LocalScale = new Vector3(0.25f);
            mesh.Transform.Rotate(0, 0, -MathHelper.PiOver2);

            var renderer = mesh.GetComponentInChildren<MeshRenderer>();
            renderer.CastShadow = true;
            renderer.ReceiveShadow = true;
            renderer.Transform.LocalScale = new Vector3(0.035f);
            renderer.Transform.Rotate(0, -MathHelper.PiOver2, 0);
            renderer.Transform.Translate(-0.1f, 0, 0);

            var modelMaterial = renderer.Material as StandardMaterial;
            modelMaterial.MainTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Diffuse");
            modelMaterial.NormalMap = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Normal");
            modelMaterial.EmissiveMap = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Emission");
            modelMaterial.SpecularTexture = Application.Content.Load<Texture2D>("Models/Quandtum/textures/Turret-Specular");
            modelMaterial.SpecularPower = 25;

            _camera.AddComponent<VRPlayerEnabler>();

            // Light
            AddLightGroundTest();
        }
    }
}