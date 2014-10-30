using C3DE.Demo.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.UI;

namespace C3DE.Demo
{
    public class AndroidGame : Engine
    {
		public AndroidGame() : base() { }

        protected override void Initialize()
        {
            GUI.Skin = DemoGame.CreateSkin(Content);

            Application.SceneManager.Add(new MenuDemo());
            Application.SceneManager.Add(new HeightmapTerrain());
			Application.SceneManager.Add(new RandomTerrain ());
            Application.SceneManager.Add(new HexagonTerrain());
            Application.LoadLevel(0);

            base.Initialize();
        }
    }
}
