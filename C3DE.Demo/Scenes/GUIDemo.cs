using C3DE.Demo.Scripts;

namespace C3DE.Demo.Scenes
{
    public class GUIDemo : Scene
    {
        public GUIDemo() : base("User Interface") { }

        public override void Initialize()
        {
            base.Initialize();

            var cameraGo = GameObjectFactory.CreateCamera();
            cameraGo.AddComponent<UIWidgetDemo>();
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);
        }
    }
}
