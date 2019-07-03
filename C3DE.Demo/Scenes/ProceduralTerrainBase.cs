using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public abstract class ProceduralTerrainBase : SimpleDemo
    {
        protected Terrain _terrain;

        public ProceduralTerrainBase(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

              // Finally a terrain
            var terrainMaterial = new StandardTerrainMaterial();
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMaterial.SandTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMaterial.SnowTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Snow");
            terrainMaterial.RockTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");

            var terrainGo = GameObjectFactory.CreateTerrain();
            _terrain = terrainGo.GetComponent<Terrain>();

            _terrain.Randomize(4, 12);
            _terrain.Renderer.Material = terrainMaterial;
            Add(terrainGo);

            _terrain.SetWeightData(0.5f, 4, 15, 30);

            terrainMaterial.WeightTexture = _terrain.GenerateWeightMap();
            terrainMaterial.Tiling = new Vector2(4);

            SetupScene();

            _camera.AddComponent<Scripts.VRPlayerEnabler>();
        }

        protected abstract void SetupScene();
    }
}
