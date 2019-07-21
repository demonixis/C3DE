using C3DE.Components;
using C3DE.Graphics.Rendering;
using C3DE.UI;

namespace C3DE.Demo.Scripts
{
    public sealed class RendererSwitcher : Behaviour
    {
        private SideMenu _sideMenu;

        public override void Start()
        {
            base.Start();

            _sideMenu = new SideMenu("Renderers", new[] { "Forward", "Deferred" }, 0);
            _sideMenu.SelectionChanged += SetRenderSelected;
        }
        
        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);
            _sideMenu.Draw(ui);
        }

        private void SetRenderSelected(int index)
        {
#if ANDROID
            return;
#endif
            var graphics = Application.GraphicsDevice;
            var engine = Application.Engine;

            if (index == 0)
                engine.Renderer = new ForwardRenderer(graphics);
            else if (index == 1)
                engine.Renderer = new DeferredRenderer(graphics);
        }
    }
}
