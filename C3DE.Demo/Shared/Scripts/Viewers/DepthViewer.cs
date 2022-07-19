using C3DE.Components;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts.Viewers
{
    public class DepthViewer : Behaviour
    {
        private Rectangle _rect;

        public override void Start()
        {
            _rect = new Rectangle(0, 0, 250, 150);
        }

        public override void OnGUI(GUI gui)
        {
            var renderer = Application.Engine.Renderer;
            var depth = renderer.GetDepthBuffer();
            if (depth != null)
                gui.DrawTexture(_rect, depth);
        }
    }
}
