using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Demo.Scripts.Viewers;
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
            var waterGo = new GameObject("Water");
            var terrain = waterGo.AddComponent<Terrain>();
            terrain.Randomize(5, 1);
            terrain.Geometry.Build();

            var content = Application.Content;
            var renderer = waterGo.GetComponent<MeshRenderer>();
            renderer.Material = new StandardWaterMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/Fluids/water"),
                NormalMap = content.Load<Texture2D>("Textures/Fluids/Water_Normal"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 50
            }; ;

            // Create the planar reflection.
            /*var planarGo = new GameObject("PlanarReflection");
            planarGo.Transform.Translate(0, 20, 0);
            var planar = planarGo.AddComponent<PlanarReflection>();
            planar.Initialize(Application.GraphicsDevice, 512);
            planar.AddComponent<ReflectionPlanarViewer>();

            // Assign to the water mesh.
            var waterRenderer = waterGo.GetComponent<Renderer>();
            waterRenderer.PlanarReflection = planar;*/

           //AddLightGroundTest();
        }
    }
}
