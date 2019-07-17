using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainWater : ProceduralTerrainBase
    {
        public ProceduralTerrainWater() : base("Procedural Terrain (Water)") { }

        protected override void SetupScene()
        {
            _directionalLight.Transform.LocalPosition = new Vector3(250, 500, 100);

            // Water
            var waterTexture = Application.Content.Load<Texture2D>("Textures/Fluids/water");
            var lavaNormal = Application.Content.Load<Texture2D>("Textures/Fluids/Water_Normal");
            var waterGo = GameObjectFactory.CreateWater(waterTexture, lavaNormal, new Vector3(_terrain.Width * 0.5f));

            var water = waterGo.GetComponent<MeshRenderer>();
            var waterMat = (StandardWaterMaterial)water.Material;
            waterMat.ReflectionTexture = _scene.RenderSettings.Skybox.Texture;
            waterMat.SpecularTexture = TextureFactory.CreateColor(Color.Gray, 1, 1);
            waterMat.ReflectionIntensity = 0.95f;

            // Create the planar reflection.
            var planarGo = new GameObject("PlanarReflection");
            planarGo.Transform.Translate(0, 20, 0);
            var planar = planarGo.AddComponent<PlanarReflection>();
            planar.Initialize(Application.GraphicsDevice, 512);
            planar.AddComponent<ReflectionPlanarViewer>();

            // Assign to the water mesh.
            var waterRenderer = waterGo.GetComponent<Renderer>();
            waterRenderer.PlanarReflection = planar;

           //AddLightGroundTest();
        }
    }
}
