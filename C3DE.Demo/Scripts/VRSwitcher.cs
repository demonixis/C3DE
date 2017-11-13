using C3DE.Components;
using C3DE.Graphics.Rendering;
using C3DE.VR;
using C3DE.UI;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scripts
{
    public class VRSwitcher : Behaviour
    {
        private bool _vrEnabled = false;
        public Action<bool> VRChanged;

        public override void Start()
        {
            base.Start();
            _vrEnabled = Application.Engine.Renderer.VREnabled;
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);

            if (ui.Button(new Rectangle(10, 10, 100, 30), "Toggle VR"))
                Toggle();
        }

        private void Toggle()
        {
            _vrEnabled = Application.Engine.Renderer.VREnabled;
            _vrEnabled = !_vrEnabled;

            if (_vrEnabled)
            {
                var vrDevice = GetService();
                if (vrDevice.TryInitialize() == 0)
                    Application.Engine.Renderer.SetVREnabled(vrDevice);
                else
                    _vrEnabled = false;
            }

            VRChanged?.Invoke(true);
        }

        private VRService GetService()
        {
#if DESKTOPGL
            return new OSVRService(Application.Engine);
#else
            return new NullVRService(Application.Engine);
#endif
        }
    }
}
