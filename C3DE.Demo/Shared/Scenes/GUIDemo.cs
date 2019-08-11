using C3DE.Demo.Scripts;

namespace C3DE.Demo.Scenes
{
    public class GUIDemo : BaseDemo
    {
        public GUIDemo() : base("IM GUI Demo") { }

        public override void Initialize()
        {
            base.Initialize();
            OptimizeFor2D();
            _camera.AddComponent<UIWidgetDemo>();
        }
    }
}
