using C3DE.Components;
using C3DE.VR;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class MotionController : Behaviour
    {
        private VRService m_VRService;
        private Vector3 m_Position;
        private Quaternion m_Rotation;

        public bool LeftHand { get; set; }

        public override void Start()
        {
            base.Start();
            m_VRService = VRManager.ActiveService;
            VRManager.VRServiceChanged += VRManager_VRServiceChanged;
        }

        private void VRManager_VRServiceChanged(VRService service)
        {
            m_VRService = service;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            VRManager.VRServiceChanged -= VRManager_VRServiceChanged;
        }

        public override void Update()
        {
            base.Update();

            if (m_VRService == null)
                return;

            m_VRService.GetHandTransform(LeftHand ? 0 : 1, ref m_Position, ref m_Rotation);
            
            m_Transform.LocalPosition = m_Position;
            m_Transform.LocalRotation = m_Rotation.ToEuler();
        }
    }
}
