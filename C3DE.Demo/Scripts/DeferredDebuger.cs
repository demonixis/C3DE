using C3DE.Components;
using C3DE.Graphics.Rendering;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class DeferredDebuger : Behaviour
    {
        private DeferredRenderer m_renderer;

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);

            if (m_renderer == null)
            {
                m_renderer = Application.Engine.Renderer as DeferredRenderer;
                if (m_renderer == null)
                    return;
            }

            var width = 320;
            var height = 200;

            ui.DrawTexture(new Rectangle(0, 0, width, height), m_renderer.ColorBuffer);
            ui.DrawTexture(new Rectangle(0, height, width, height), m_renderer.NormalMap);
            ui.DrawTexture(new Rectangle(0, height * 2, width, height), m_renderer.DepthBuffer);
            ui.DrawTexture(new Rectangle(0, height * 3, width, height), m_renderer.LightMap);
        }
    }
}
