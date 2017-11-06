using C3DE.Components.Rendering;
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
            // Water
            var waterTexture = Application.Content.Load<Texture2D>("Textures/water");
            var lavaNormal = Application.Content.Load<Texture2D>("Textures/wavesbump");
            var waterGo = GameObjectFactory.CreateWater(waterTexture, lavaNormal, new Vector3(m_Terrain.Width * 0.5f));
            Add(waterGo);

            var water = waterGo.GetComponent<MeshRenderer>();
            var waterMat = (WaterMaterial)water.Material;
            waterMat.ReflectiveMap = scene.RenderSettings.Skybox.Texture;
            waterMat.WaterTransparency = 0.6f;
        }
    }
}
