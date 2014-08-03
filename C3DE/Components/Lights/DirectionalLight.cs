using Microsoft.Xna.Framework;

namespace C3DE.Components.Lights
{
    public class DirectionalLight : Light
    {
        public Vector3 Direction { get; set; }

        public DirectionalLight()
            : this(null)
        {
        }

        public DirectionalLight(SceneObject sceneObject)
            : base(sceneObject)
        {
            Direction = new Vector3(1, 1, 0);
        }
    }
}
