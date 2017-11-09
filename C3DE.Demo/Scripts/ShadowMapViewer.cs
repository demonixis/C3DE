using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    class ShadowMapViewer : Behaviour
    {
        private Rectangle _rect;
        private ShadowGenerator _generator;

        public override void Start()
        {
            _rect = new Rectangle(0, 0, 150, 150);

            var light = GetComponent<Light>();
            _generator = light.ShadowGenerator;
        }

        public override void OnGUI(GUI gui)
        {
            gui.DrawTexture(_rect, _generator.ShadowMap);
        }
    }
}
