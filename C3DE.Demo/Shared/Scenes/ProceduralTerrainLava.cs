using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainLava : ProceduralTerrainBase
    {
        public ProceduralTerrainLava() : base("Procedural Terrain (Lava)") { }

        protected override void SetupScene()
        {
            var content = Application.Content;
            var lavaTexture = content.Load<Texture2D>("Textures/Fluids/lava_texture");
            var lavaNormal = content.Load<Texture2D>("Textures/Fluids/lava_bump");
            var lava = GameObjectFactory.CreateLava(lavaTexture, lavaNormal, new Vector3(_terrain.Width * 0.5f));
            Add(lava);
        }
    }
}