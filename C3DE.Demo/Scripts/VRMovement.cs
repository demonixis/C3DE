using C3DE.Components;
using C3DE.VR;

namespace C3DE.Demo.Scripts
{
    public sealed class VRMovement : Behaviour
    {
        public float Speed { get; set; } = 1.5f;

        public override void Update()
        {
            var service = VRManager.ActiveService;
            if (service == null)
                return;

            var h = service.GetAxis(0, XRAxis.TouchpadX);
            var v = service.GetAxis(0, XRAxis.TouchpadY);

            m_Transform.Translate(h * Speed * Time.DeltaTime, 0, v * Speed * Time.DeltaTime);
        }
    }
}
