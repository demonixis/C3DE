using C3DE.Components;
using C3DE.Graphics.PostProcessing;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts.Viewers
{
    public class SSAOViewer : Behaviour
    {
        private RenderTarget2D _ssaoTexture;
        private RenderTarget2D _depthTexture;

        public override void Start()
        {
        }

        public override void OnGUI(GUI ui)
        {

        }
    }
}
