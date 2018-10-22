using C3DE.Components;
using C3DE.VR;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class VRPlayerEnabler : Behaviour
    {
        private Transform m_PlayerTransform;
        private Transform m_CameraTransform;

        public Vector3 Position { get; set; }

        public override void Start()
        {
            VRManager.VRServiceChanged += OnVRChanged;

            var player = GameObject.Find("Player");
            if (player == null)
                player = new GameObject("Player");

            player.AddComponent<VRMovement>();

            m_PlayerTransform = player.Transform;
            m_CameraTransform = Camera.Main.Transform;
        }

        private void OnVRChanged(VRService service)
        {
            if (service != null)
            {
                m_PlayerTransform.Position = Position;
                m_CameraTransform.Parent = m_PlayerTransform;
            }
            else
                m_CameraTransform.Parent = null;
        }
    }
}
