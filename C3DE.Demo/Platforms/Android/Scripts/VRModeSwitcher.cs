using System;
using C3DE.UI;
using Microsoft.Xna.Framework;
using C3DE.Components.Controllers;
using C3DE.Components;

namespace C3DE.Demo
{
    public class VRModeSwitcher : Behaviour
    {
        private Engine _engine;
        private Renderer _basicRenderer;
        private VRMobileRenderer _vrMobileRenderer;
        private Rectangle _switchRect;
        private bool _vrEnabled;
        private OrientationController _orientationController;

        public override void Start()
        {
            var bWidth = 200;
            var bHeight = 50;

            _switchRect = new Rectangle(Screen.WidthPerTwo - bWidth / 2, Screen.Height - bHeight * 2, bWidth, bHeight);
            _vrEnabled = false;

            _engine = Application.Game as Engine;
            _basicRenderer = _engine.Renderer as Renderer;
            _vrMobileRenderer = new VRMobileRenderer();
            _vrMobileRenderer.Initialize(Application.Content);

            _orientationController = AddComponent<OrientationController>();
            _orientationController.Enabled = false;
        }

        public override void OnDestroy()
        {
            if (_vrEnabled)
                _engine.Renderer = _basicRenderer;
        }

        public override void OnGUI(GUI ui)
        {
            if (ui.Button(ref _switchRect, _vrEnabled ? "Normal Mode" : "VR Mode"))
            {
                if (_vrEnabled)
                    _engine.Renderer = _basicRenderer;
                else
                    _engine.Renderer = _vrMobileRenderer;

                _orientationController.Enabled = _vrEnabled;

                // FIXME
                if (!_orientationController.Initialized)
                {
                    _orientationController.Start();
                }
            }
        }
    }
}

