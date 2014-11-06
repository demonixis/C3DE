using C3DE.Components.Controllers;
using C3DE.Demo.Scripts;
using C3DE.Prefabs;
using C3DE.UI;

namespace C3DE.Demo.Scenes
{
    public class MenuDemo : Scene
    {
        public MenuDemo() : base("Menu demo") { }

        public override void Initialize()
        {
            base.Initialize();

#if ANDROID
            //MenuBehaviour.ButtonWidth = 250;
            //MenuBehaviour.ButtonHeight = 75;
#endif

            GUI.Skin = DemoGame.CreateSkin(Application.Content);

            var camera = new CameraPrefab("cam");
            Add(camera);

            var sceneObject = new SceneObject("MenuNode");
            sceneObject.AddComponent<MenuBehaviour>();
            Add(sceneObject);
        }
    }
}
