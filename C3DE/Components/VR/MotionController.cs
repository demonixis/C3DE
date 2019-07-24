using C3DE.VR;
using Microsoft.Xna.Framework;

namespace C3DE.Components.VR
{
    public class MotionController : Behaviour
    {
        private VRService _service;
        private Vector3 _position;
        private Quaternion _rotation;

        public bool LeftHand { get; set; }

        public override void Start()
        {
            _service = VRManager.ActiveService;
            VRManager.VRServiceChanged += OnVRServiceChanged;
        }

        private void OnVRServiceChanged(VRService service)
        {
            _service = service;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            VRManager.VRServiceChanged -= OnVRServiceChanged;
        }

        public override void Update()
        {
            if (_service == null)
                return;

            _service.GetLocalPosition(LeftHand ? 0 : 1, ref _position);
            _service.GetLocalRotation(LeftHand ? 0 : 1, ref _rotation);

            _transform.LocalPosition = _position;
            _transform.LocalRotation = _rotation.ToEuler();
        }
    }
}
