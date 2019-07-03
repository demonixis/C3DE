using C3DE.Components;
using C3DE.VR;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class MotionController : Behaviour
    {
        private VRService _VRService;
        private Vector3 _position;
        private Quaternion _rotation;

        public bool LeftHand { get; set; }

        public override void Start()
        {
            base.Start();
            _VRService = VRManager.ActiveService;
            VRManager.VRServiceChanged += VRManager_VRServiceChanged;
        }

        private void VRManager_VRServiceChanged(VRService service)
        {
            _VRService = service;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            VRManager.VRServiceChanged -= VRManager_VRServiceChanged;
        }

        public override void Update()
        {
            base.Update();

            if (_VRService == null)
                return;

            _VRService.GetLocalPosition(LeftHand ? 0 : 1, ref _position);
            _VRService.GetLocalRotation(LeftHand ? 0 : 1, ref _rotation);

            _transform.LocalPosition = _position;
            _transform.LocalRotation = _rotation.ToEuler();
        }
    }
}
