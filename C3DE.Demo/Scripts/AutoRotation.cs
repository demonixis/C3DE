using C3DE.Components;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class AutoRotation : Component
    {
        public Vector3 Rotation { get; set; }

        public override void Update()
        {
            sceneObject.Transform.Rotate(Rotation);
        }
    }
}
