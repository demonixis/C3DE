using C3DE.Components;
using C3DE.Graphics.Rendering;
using C3DE.UI;

namespace C3DE.Demo.Scripts
{
    public sealed class RendererSwitcher : Behaviour
    {
        private SideMenu m_SideMenu;

        public override void Start()
        {
            base.Start();

            m_SideMenu = new SideMenu("Renderers", new[] { "Forward", "Deferred", "Light PrePass" }, 0);
            m_SideMenu.SelectionChanged += SetRenderSelected;
        }
        
        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);
            m_SideMenu.Draw(ui);
        }

        private void SetRenderSelected(int index)
        {
            var graphics = Application.GraphicsDevice;
            var engine = Application.Engine;

            if (index == 0)
                engine.Renderer = new ForwardRenderer(graphics);
            else if (index == 1)
                engine.Renderer = new DeferredRenderer(graphics);
            else if (index == 2)
                engine.Renderer = new LightPrePassRenderer(graphics);
        }
    }
}
