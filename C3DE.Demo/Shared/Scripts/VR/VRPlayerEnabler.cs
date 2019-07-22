using C3DE.Components;
using C3DE.VR;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts.VR
{
    public class VRPlayerEnabler : Behaviour
    {
        private Transform _payerTransform;
        private Transform _cameraTransform;

        public Vector3 Position { get; set; }

        public override void Start()
        {
            VRManager.VRServiceChanged += OnVRChanged;

            var player = GameObject.Find("Player");
            if (player == null)
                player = new GameObject("Player");

            player.AddComponent<VRMovement>();

            _payerTransform = player.Transform;
            _cameraTransform = Camera.Main.Transform;
        }

        private void OnVRChanged(VRService service)
        {
            if (service != null)
            {
                _payerTransform.Position = Position;
                _cameraTransform.Parent = _payerTransform;
            }
            else
                _cameraTransform.Parent = null;
        }
    }
}
