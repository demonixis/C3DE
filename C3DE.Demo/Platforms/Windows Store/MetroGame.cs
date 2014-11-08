using C3DE.Demo.Scenes;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo
{
    public class MetroGame : Engine
    {
        public MetroGame() : base() { }

        protected override void Initialize()
        {
            Screen.SetVirtualResolution(1024, 600);
            GUI.Skin = DemoGame.CreateSkin(Content);

            Application.SceneManager.Add(new MenuDemo(), true);
            Application.SceneManager.Add(new HeightmapDemo());
            Application.SceneManager.Add(new ProceduralTerrainWater());
            Application.SceneManager.Add(new ProceduralTerrainLava());
            Application.SceneManager.Add(new HalloweenDemo());
            Application.SceneManager.Add(new HexagonTerrainDemo());
            Application.LoadLevel(0);

            base.Initialize();
        }
    }
}
