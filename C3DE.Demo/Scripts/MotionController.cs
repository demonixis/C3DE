using C3DE.Components;
using C3DE.VR;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class MotionController : Behaviour
    {
        private Vector3 position;
        private Quaternion rotation;

        public bool LeftHand { get; set; }

        public override void Update()
        {
            var service = VRManager.ActiveService;
            if (service == null)
                return;

            service.GetHandTransform(LeftHand ? 0 : 1, ref position, ref rotation);

            transform.Position = position;
            transform.Rotation = rotation.ToEuler();
        }
    }
}
