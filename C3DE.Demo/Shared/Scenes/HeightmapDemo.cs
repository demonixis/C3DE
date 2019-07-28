using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Demo.Scripts.Viewers;
using C3DE.Demo.Scripts.VR;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class HeightmapDemo : SimpleDemo
    {
        public HeightmapDemo() : base("Heightmap Terrain") { }

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.LoadHeightmap("Textures/heightmap");

            var weightMap = terrain.GenerateWeightMap();
            terrain.Renderer.Material = GetTerrainMaterial(content, weightMap, false);
            terrainGo.AddComponent<WeightMapViewer>();

            // Water
            var waterGo = new GameObject("Water");
            waterGo.Transform.Translate(0, 10, 0);
            var terrainW = waterGo.AddComponent<Terrain>();
            terrainW.Randomize(5, 2);
            terrainW.Geometry.Build();

            // Reflection Probe
            var probe = GameObjectFactory.CreateReflectionProbe(new Vector3(0, 35, 0));

            var renderer = waterGo.GetComponent<MeshRenderer>();
            renderer.Material = new StandardWaterMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/Fluids/water"),
                NormalMap = content.Load<Texture2D>("Textures/Fluids/Water_Normal"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 50,
                ReflectionMap = probe.ReflectionMap,
                ReflectionIntensity = 0.75f
            };

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.None;
            //RenderSettings.Skybox.FogSupported = true;
            //RenderSettings.Skybox.OverrideSkyboxFog(FogMode.Exp2, 0.05f, 0, 0);

            var vrPlayerEnabler = _camera.AddComponent<VRPlayerEnabler>();
            vrPlayerEnabler.Position = new Vector3(0, waterGo.Transform.Position.Y + 0.5f, 0);
        }
    }
}