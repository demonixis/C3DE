using C3DE.Demo.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo
{
    public class AndroidGame : Engine
    {
		public AndroidGame() : base() { 

		}

        protected override void Initialize()
        {
            Application.SceneManager.Add(new MenuDemo(), true);
            Application.SceneManager.Add(new HeightmapDemo());
            Application.SceneManager.Add(new RandomTerrain());
            Application.SceneManager.Add(new GridDemo());
            Application.SceneManager.Add(new HexagonTerrainDemo());
            Application.LoadLevel(1);
            base.Initialize();
        }
    }
}
