using System;
using C3DE.UI;
using Microsoft.Xna.Framework;
using C3DE.Components;
using C3DE.Components.Controllers;

namespace C3DE.Demo
{
    public class VRModeSwitcher : Behaviour
    {
        private Renderer _basicRenderer;
        private VRMobileRenderer _vrMobileRenderer;
        private Rectangle _switchRect;
        private bool _vrEnabled;
        private Controller _baseController;
        private OrientationController _orientationController;
        private Point _originalResolution;

        public override void Start()
        {
            _originalResolution = new Point(Screen.Width, Screen.Height);

            var bWidth = 200;
            var bHeight = 50;

            _switchRect = new Rectangle(Screen.WidthPerTwo - bWidth / 2, Screen.Height - bHeight * 2, bWidth, bHeight);
            _vrEnabled = false;

            _basicRenderer = Application.Engine.Renderer as Renderer;
            _vrMobileRenderer = new VRMobileRenderer();
            _vrMobileRenderer.Initialize(Application.Content);

            _baseController = GetComponent<Controller>();
            _orientationController = AddComponent<OrientationController>();
            _orientationController.Start();
            _orientationController.Enabled = false;

            Screen.Setup(_originalResolution.X, _originalResolution.Y, null, null);
        }

        public override void OnDestroy()
        {
            if (_vrEnabled)
                Application.Engine.Renderer = _basicRenderer;
        }

        public override void OnGUI(GUI ui)
        {
            if (ui.Button(ref _switchRect, _vrEnabled ? "Normal Mode" : "VR Mode"))
            {
                _vrEnabled = !_vrEnabled;

                if (_vrEnabled)
                    Application.Engine.Renderer = _basicRenderer;
                else
                    Application.Engine.Renderer = _vrMobileRenderer;

                Screen.Setup(_vrEnabled ? _originalResolution.X >> 1 : _originalResolution.X, _originalResolution.Y, null, null);
                    
                if (_vrEnabled)
                {
                    _orientationController.Enabled = _vrEnabled;

                    if (_baseController != null)
                        _baseController.Enabled = false; 
                }
                else if (_baseController != null)
                    _baseController.Enabled = true;
            }
        }
    }
}

