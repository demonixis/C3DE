using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
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
        private GameObject[] m_Hands;

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

            var handMaterial = new UnlitMaterial();
            handMaterial.DiffuseColor = Color.AliceBlue;

            m_Hands = new GameObject[2];

            CreateHand(0, handMaterial);
            CreateHand(1, handMaterial);
        }

        public override void OnDestroy()
        {
            Application.Engine.Renderer.SetVREnabled(false);
        }

        private void CreateHand(int id, Material material)
        {
            m_Hands[id] = new GameObject($"Hand_{id}");
            m_Hands[id].AddComponent<MotionController>().LeftHand = id == 0;

            var renderer = m_Hands[id].AddComponent<MeshRenderer>();
            renderer.Geometry = new CubeMesh();
            renderer.Geometry.Size = new Vector3(0.1f);
            renderer.Geometry.Build();
            renderer.Material = material;

            m_Hands[id].Enabled = false;

            //m_Hands[id].Transform.Parent = Camera.Main.Transform;
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

            foreach (var hand in m_Hands)
                hand.Enabled = m_VREnabled;

            VRChanged?.Invoke(m_VREnabled);
        }
    }
}
