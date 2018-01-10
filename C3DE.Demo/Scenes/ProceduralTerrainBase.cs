using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public abstract class ProceduralTerrainBase : SimpleDemo
    {
        protected Terrain m_Terrain;

        public ProceduralTerrainBase(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

              // Finally a terrain
            var terrainMaterial = new StandardTerrainMaterial(m_Scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMaterial.SandTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMaterial.SnowTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Snow");
            terrainMaterial.RockTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");

            var terrainGo = GameObjectFactory.CreateTerrain();
            m_Terrain = terrainGo.GetComponent<Terrain>();

            m_Terrain.Randomize(4, 12);
            m_Terrain.Renderer.Material = terrainMaterial;
            Add(terrainGo);

            m_Terrain.SetWeightData(0.5f, 4, 15, 30);

            terrainMaterial.WeightTexture = m_Terrain.GenerateWeightMap();
            terrainMaterial.Tiling = new Vector2(4);

            // Sun Flares
            var content = Application.Content;
            var glowTexture = content.Load<Texture2D>("Textures/Flares/glow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/flare1"),
                content.Load<Texture2D>("Textures/Flares/flare2"),
                content.Load<Texture2D>("Textures/Flares/flare3")
            };
            var direction = m_DirectionalLight.Transform.LocalRotation;
            direction.Normalize();

            var sunflares = m_Camera.AddComponent<LensFlare>();
            sunflares.LightDirection = direction;
            sunflares.Setup(glowTexture, flareTextures);

            SetupScene();
        }

        protected abstract void SetupScene();
    }
}
