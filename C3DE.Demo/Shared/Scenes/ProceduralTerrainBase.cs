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

            var content = Application.Content;

            // Finally a terrain
            var terrainMaterial = new StandardTerrainMaterial();
            terrainMaterial.MainTexture = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_col");
            terrainMaterial.NormalMap = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_nrm");
            terrainMaterial.SandTexture = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_col");
            terrainMaterial.SandNormalMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_nrm");
            terrainMaterial.SnowTexture = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_col");
            terrainMaterial.SnownNormalMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_nrm");
            terrainMaterial.RockTexture = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_col");
            terrainMaterial.RockNormalMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_nrm");

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
