using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class TerrainLavaPBRDemo : ProceduralTerrainBase
    {
        public TerrainLavaPBRDemo() : base("Procedural PBR Terrain (Lava)") { }

        protected override void SetupScene()
        {
            var content = Application.Content;
            var lavaMaterial = new PBRLavaMaterial();
            lavaMaterial.MainTexture = content.Load<Texture2D>("Textures/Fluids/lava_texture");
            lavaMaterial.NormalMap = content.Load<Texture2D>("Textures/Fluids/wavesbump");
            lavaMaterial.Metallic = 0;
            lavaMaterial.Roughness = 0;

            // Lava
            var lava = GameObjectFactory.CreateLava(null, null, new Vector3(_terrain.Width * 0.5f));
            lava.GetComponent<Renderer>().Material = lavaMaterial;
        }
    }
}