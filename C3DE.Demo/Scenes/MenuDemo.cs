using C3DE.Demo.Scripts;
using C3DE.UI;

namespace C3DE.Demo.Scenes
{
    public class MenuDemo : Scene
    {
        public MenuDemo() : base("Menu demo") { }

        public override void Initialize()
        {
            base.Initialize();

            GUI.Skin = DemoGame.CreateSkin(Application.Content);

            Add(new C3DE.Prefabs.CameraPrefab("cam"));

#if ANDROID
            MenuBehaviour.ButtonWidth = 250;
            MenuBehaviour.ButtonHeight = 75;
#endif

            var sceneObject = new SceneObject("MenuNode");
            sceneObject.AddComponent<MenuBehaviour>();
            Add(sceneObject);
        }
    }
}
