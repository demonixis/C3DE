using C3DE.Components;
using C3DE.Rendering;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class VRRendererSwitcher : Behaviour
    {
        private Renderer _basicRenderer;
        private VRRenderer _vrRenderer;

        public override void Start()
        {
            _basicRenderer = (Renderer)Application.Engine.Renderer;
            _vrRenderer = new VRRenderer();
            _vrRenderer.Initialize(Application.Content);
            Screen.Setup(Application.GraphicsDeviceManager.PreferredBackBufferWidth, Application.GraphicsDeviceManager.PreferredBackBufferHeight, null, null);
        }

        public override void Update()
        {
            if (Input.Keys.JustPressed(Keys.Space))
            {
                if (Application.Engine.Renderer is VRRenderer)
                {
                    Screen.Setup(Application.GraphicsDeviceManager.PreferredBackBufferWidth, Application.GraphicsDeviceManager.PreferredBackBufferHeight, null, null);
                    Application.Engine.Renderer = _basicRenderer;
                }
                else
                {
                    Screen.Setup(Application.GraphicsDeviceManager.PreferredBackBufferWidth / 2, Application.GraphicsDeviceManager.PreferredBackBufferHeight, null, null);
                    Application.Engine.Renderer = _vrRenderer;
                }
            }
        }
    }
}
