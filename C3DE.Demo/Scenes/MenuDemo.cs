using C3DE.Demo.Scripts;
using C3DE.UI;

namespace C3DE.Demo.Scenes
{
    public class MenuDemo : SimpleDemo
    {
        public MenuDemo() : base("Menu") { }

        public override void Initialize()
        {
            base.Initialize();
            OptimizeFor2D();
            GUI.Skin = DemoGame.CreateSkin(Application.Content, false);
            m_Camera.AddComponent<MenuBehaviour>();
        }
    }
}
