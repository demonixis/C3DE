using C3DE.Demo.Scripts;
using C3DE.UI;

namespace C3DE.Demo.Scenes
{
    public class MenuDemo : BaseDemo
    {
        public MenuDemo() : base("Menu") { }

        public override void Initialize()
        {
            base.Initialize();
            OptimizeFor2D();
            GUI.Skin = DemoGame.CreateSkin(Application.Content, false);
            _camera.AddComponent<MenuBehaviour>();

            var adapter = Application.GraphicsDevice.Adapter.Description;
            if (adapter.ToLower().Contains("intel"))
                Graphics.Rendering.ForwardRenderer.MaxLightCount = 16;
        }
    }
}
