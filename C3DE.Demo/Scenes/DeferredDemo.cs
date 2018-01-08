using C3DE.Components;
using C3DE.Components.Lighting;
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

            var directionalLight = GameObject.Find("DirectionalLight");
            if (directionalLight != null)
                directionalLight.GetComponent<Light>().Intensity = 0.1f;

            Application.Engine.Renderer = new DeferredRenderer(Application.GraphicsDevice);

            Camera.Main.AddComponent<DeferredDebuger>();
            Camera.Main.AddComponent<PostProcessSwitcher>();
        }

        public override void Unload()
        {
            base.Unload();
            Application.Engine.Renderer = new ForwardRenderer(Application.GraphicsDevice);
        }
    }
}
