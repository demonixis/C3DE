using Microsoft.Xna.Framework;

namespace C3DE.Components.Lights
{
    public class PointLight: Light
    {
        public Vector3 Direction;
        public float Angle;
        public int exponent;

        public PointLight()
            : this(null)
        {
        }

        public PointLight(SceneObject sceneObject)
            : base(sceneObject)
        {
        }
    }
}
