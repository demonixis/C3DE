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

            Add(new C3DE.Prefabs.CameraPrefab("cam"));

            var sceneObject = new SceneObject("MenuNode");
            sceneObject.AddComponent<MenuBehaviour>();
            Add(sceneObject);
        }
    }
}
