using C3DE.Components.Lights;
using Microsoft.Xna.Framework;

namespace C3DE
{
    public class LightPrefab : SceneObject
    {
        private Light _light;
        private LightType _type;

        public LightPrefab()
            : this(null)
        {

        }

        public LightPrefab(string name)
            : base()
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;

            _light = new Light(this);
        }

        public void SetLighType(LightType type)
        {
            if (type == LightType.Directional)
                _light = new DirectionalLight(this);
            else if (type == LightType.Point)
                _light = new PointLight(this);

            _type = type;
        }
    }
}
