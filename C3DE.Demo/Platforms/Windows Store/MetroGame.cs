using C3DE.Demo.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo
{
    public class MetroGame : Engine
    {
        public MetroGame() : base() { }

        protected override void Initialize()
        {
            Application.SceneManager.Add(new MenuDemo(), true);
            Application.SceneManager.Add(new HeightmapDemo());
            Application.SceneManager.Add(new ProceduralTerrainWater());
            Application.SceneManager.Add(new ProceduralTerrainLava());
            Application.SceneManager.Add(new LightingDemo());
            Application.SceneManager.Add(new HexagonTerrainDemo());
            Application.LoadLevel(0);
            base.Initialize();
        }
    }
}
