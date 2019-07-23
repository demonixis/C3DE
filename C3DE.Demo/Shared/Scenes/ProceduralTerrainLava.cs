using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainLava : ProceduralTerrainBase
    {
        public ProceduralTerrainLava() : base("Procedural Terrain (Lava)") { }

        protected override void SetupScene()
        {
            // Lava
            var lavaGo = new GameObject("Lava");
            var terrain = lavaGo.AddComponent<Terrain>();
            terrain.Randomize(5, 1);
            terrain.Geometry.Build();

            var content = Application.Content;
            var renderer = lavaGo.GetComponent<MeshRenderer>();
            renderer.Material = new StandardLavaMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/Fluids/lava_texture"),
                NormalMap = content.Load<Texture2D>("Textures/Fluids/wavesbump"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 50
            };
        }
    }
}