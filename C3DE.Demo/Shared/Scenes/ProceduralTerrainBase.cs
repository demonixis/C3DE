using C3DE.Components.Rendering;
using C3DE.Demo.Scripts.VR;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public abstract class ProceduralTerrainBase : SimpleDemo
    {
        protected Terrain _terrain;
        protected bool _pbr;

        public ProceduralTerrainBase(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            _terrain = GameObjectFactory.CreateTerrain();
            _terrain.Randomize(4, 12);
            _terrain.SetWeightData(0.5f, 4, 15, 30);
            _terrain.Renderer.Material = GetTerrainMaterial(content, _terrain.GenerateWeightMap(), _pbr);

            SetupScene();

            _camera.AddComponent<VRPlayerEnabler>();
        }

        protected abstract void SetupScene();
    }
}
