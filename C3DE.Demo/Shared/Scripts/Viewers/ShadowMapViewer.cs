using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts.Viewers
{
    public class ShadowMapViewer : Behaviour
    {
        private Rectangle _rect;
        public Light _light;

        public override void Start()
        {
            _rect = new Rectangle(0, 0, 250, 150);
            _light = GetComponent<Light>();
        }

        public override void OnGUI(GUI gui)
        {
            if (_light != null)
                gui.DrawTexture(_rect, _light.ShadowGenerator.ShadowMap);
        }
    }
}
