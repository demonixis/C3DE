using C3DE.Demo.Scripts;
using C3DE.Prefabs;

namespace C3DE.Demo.Scenes
{
    public class GUIDemo : Scene
    {
        public GUIDemo() : base("User Interface") { }

        public override void Initialize()
        {
            base.Initialize();

            var camera = new CameraPrefab("Camera");
            camera.AddComponent<UIWidgetDemo>();
            camera.AddComponent<DemoBehaviour>();
            Add(camera);
        }
    }
}
