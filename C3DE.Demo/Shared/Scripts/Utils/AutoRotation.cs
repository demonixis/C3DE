using C3DE.Components;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts.Utils
{
    public class AutoRotation : Component
    {
        public Vector3 Rotation { get; set; }

        public override void Update()
        {
            _gameObject.Transform.Rotate(Rotation * Time.DeltaTime);
        }
    }
}
