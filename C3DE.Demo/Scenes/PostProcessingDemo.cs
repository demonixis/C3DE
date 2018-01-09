using C3DE.Demo.Scripts;
using C3DE.Components;

namespace C3DE.Demo.Scenes
{
    public class PostProcessingDemo : LightingDemo
    {
        public PostProcessingDemo() : base("Post Processing") { }

        public override void Initialize()
        {
            base.Initialize();

            Camera.Main.AddComponent<PostProcessSwitcher>();

            var movers = FindObjectsOfType<LightMover>();
            foreach (var mover in movers)
                Destroy(mover);

            var switchers = FindObjectsOfType<LightSwitcher>();
            foreach (var switcher in switchers)
                Destroy(switcher);
        }
    }
}
