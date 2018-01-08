using C3DE.Components;
using C3DE.UI;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scripts
{
    public class VRSwitcher : Behaviour
    {
        private Rectangle m_UIRectangle;
        private bool m_VREnabled = false;
        public Action<bool> VRChanged;

        public Point UIPosition
        {
            get => new Point(m_UIRectangle.X, m_UIRectangle.Y);
            set
            {
                m_UIRectangle.X = value.X;
                m_UIRectangle.Y = value.Y;
            }
        }

        public override void Start()
        {
            base.Start();
            m_VREnabled = Application.Engine.Renderer.VREnabled;
            m_UIRectangle = new Rectangle(10, 10, 100, 30);
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);

            if (ui.Button(m_UIRectangle, "Toggle VR"))
                Toggle();
        }

        private void Toggle()
        {
            m_VREnabled = Application.Engine.Renderer.VREnabled;
            m_VREnabled = !m_VREnabled;

            if (m_VREnabled)
                m_VREnabled = Application.Engine.Renderer.SetVREnabled(true);

            VRChanged?.Invoke(m_VREnabled);
        }
    }
}
