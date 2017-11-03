using C3DE.Demo.Scripts;
using C3DE.Prefabs;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class MenuDemo : Scene
    {
        public MenuDemo() : base("Menu") { }

        public override void Initialize()
        {
            base.Initialize();

            GUI.Skin = DemoGame.CreateSkin(Application.Content, false);

            var camera = new CameraPrefab("cam");
            camera.AddComponent<MenuBehaviour>();
            Add(camera);
        }
    }
}
