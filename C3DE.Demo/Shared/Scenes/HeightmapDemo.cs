using C3DE.Components.Rendering;
using C3DE.Demo.Scripts.Viewers;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class HeightmapDemo : BaseDemo
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
            terrain.Renderer.Material = GetTerrainMaterial(content, weightMap);
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
            renderer.Material = GetWaterMaterial(content, probe.ReflectionMap);

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.None;
            //RenderSettings.Skybox.FogSupported = true;
            //RenderSettings.Skybox.OverrideSkyboxFog(FogMode.Exp2, 0.05f, 0, 0);

            _vrPlayerEnabler.Position = new Vector3(0, waterGo.Transform.Position.Y + 0.5f, 0);
        }
    }
}