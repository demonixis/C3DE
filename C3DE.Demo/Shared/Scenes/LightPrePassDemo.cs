using C3DE.Demo.Scripts;
using C3DE.Graphics.Rendering;

namespace C3DE.Demo.Scenes
{
    public class LightPrePassDemo : SponzaDemo
    {
        public LightPrePassDemo() : base("LightPrePass Rendering") { }

        public override void Initialize()
        {
            base.Initialize();
            Application.Engine.Renderer = new LightPrePassRenderer(Application.GraphicsDevice);

            _camera.AddComponent<LightPrePassDebuger>();
        }

        public override void Unload()
        {
            base.Unload();
            Application.Engine.Renderer = new ForwardRenderer(Application.GraphicsDevice);
        }
    }
}
