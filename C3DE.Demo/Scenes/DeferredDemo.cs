using C3DE.Components;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Rendering;

namespace C3DE.Demo.Scenes
{
    public class DeferredDemo : SponzaDemo
    {
        public DeferredDemo() : base("Deferred Rendering") { }

        public override void Initialize()
        {
            base.Initialize();

            Application.Engine.Renderer = new DeferredRenderer(Application.GraphicsDevice);

            Camera.Main.AddComponent<DeferredDebuger>();
        }

        public override void Unload()
        {
            base.Unload();
            Application.Engine.Renderer = new ForwardRenderer(Application.GraphicsDevice);
        }
    }
}
