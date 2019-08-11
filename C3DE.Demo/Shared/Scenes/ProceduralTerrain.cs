using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrain : BaseDemo
    {
        private bool _lava;

        public ProceduralTerrain(string name, bool lava) 
            : base(name)
        {
            _lava = lava;
        }

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            var terrain = GameObjectFactory.CreateTerrain();
            terrain.Randomize(4, 12);
            terrain.SetWeightData(0.5f, 4, 15, 30);
            terrain.Renderer.Material = GetTerrainMaterial(content, terrain.GenerateWeightMap());

            // Reflection Probe
            var probe = GameObjectFactory.CreateReflectionProbe(new Vector3(0, 50, 0));

            // Fluid
            var fluidGo = new GameObject($"{(_lava ? "Lava" : "Water")}");
            var fuildTerrain = fluidGo.AddComponent<Terrain>();
            fuildTerrain.Randomize(5, 1);
            fuildTerrain.Geometry.Build();
            fuildTerrain.Renderer.Material = _lava ? GetLavaMaterial(content) : GetWaterMaterial(content, probe.ReflectionMap);

            if (!_lava)
                _directionalLight.Transform.LocalPosition = new Vector3(250, 500, 100);
        }
    }
}
