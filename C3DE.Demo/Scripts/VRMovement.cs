using C3DE.Components;
using C3DE.VR;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public sealed class VRMovement : Behaviour
    {
        public float Speed { get; set; } = 2.5f;
        public float Rotation { get; set; } = 2.5f;

        public override void Update()
        {
            var service = VRManager.ActiveService;
            if (service == null)
                return;

            var h = service.GetAxis(0, XRAxis.TouchpadX);
            var v = service.GetAxis(0, XRAxis.TouchpadY);
            var vRight = service.GetAxis(1, XRAxis.TouchpadY);

            var translation = new Vector3(-h * Speed * Time.DeltaTime, vRight * Speed * Time.DeltaTime, v * Speed * Time.DeltaTime);
            var transformedTranslation = Vector3.Transform(translation, Matrix.CreateRotationY(m_Transform.LocalRotation.Y));

            // Translate and rotate
            m_Transform.Translate(transformedTranslation);
            m_Transform.Rotate(0, -service.GetAxis(1, XRAxis.TouchpadX) * Time.DeltaTime * Rotation, 0);
        }
    }
}
