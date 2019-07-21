using C3DE.Components;
using C3DE.Graphics.PostProcessing;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class SSAOViewer : Behaviour
    {
        private RenderTarget2D _ssaoTexture;
        private RenderTarget2D _depthTexture;

        public override void Start()
        {
            var ssao = Scene.current.GetEffect<SSAO>();
            _ssaoTexture = ssao?.SSAOTexture;
            _depthTexture = Application.Engine.Renderer.GetDepthBuffer();
        }

        public override void OnGUI(GUI ui)
        {
            if (_ssaoTexture != null)
            {
                var offset = 256;
                ui.DrawTexture(new Rectangle(0, 0, offset, offset), _ssaoTexture);
                ui.DrawTexture(new Rectangle(0, offset, offset, offset), _depthTexture);
            }
        }
    }
}
