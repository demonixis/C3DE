using C3DE.Components;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Demo.Scripts
{
    public class VRRendererSwitcher : Behaviour
    {
        private Engine _engine;
        private Renderer _basicRenderer;
        private VRRenderer _vrRenderer;

        public override void Start()
        {
            _engine = (Application.Game as Engine);
            _basicRenderer = (Renderer)_engine.Renderer;
            _vrRenderer = new VRRenderer();
            _vrRenderer.Initialize(Application.Content);
            Screen.Setup(Application.GraphicsDeviceManager.PreferredBackBufferWidth, Application.GraphicsDeviceManager.PreferredBackBufferHeight, null, null);
        }

        public override void Update()
        {
            if (Input.Keys.JustPressed(Keys.Space))
            {
                if (_engine.Renderer is VRRenderer)
                {
                    Screen.Setup(Application.GraphicsDeviceManager.PreferredBackBufferWidth, Application.GraphicsDeviceManager.PreferredBackBufferHeight, null, null);
                    _engine.Renderer = _basicRenderer;
                }
                else
                {
                    Screen.Setup(Application.GraphicsDeviceManager.PreferredBackBufferWidth / 2, Application.GraphicsDeviceManager.PreferredBackBufferHeight, null, null);
                    _engine.Renderer = _vrRenderer;
                }
            }
        }
    }
}
