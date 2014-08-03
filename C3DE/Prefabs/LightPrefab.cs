using C3DE.Components.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Prefabs
{
    public class LightPrefab : Prefab
    {
        private Light _light;
        private LightType _type;

        public bool EnableShadows
        {
            get { return _light.shadowGenerator.Enabled; }
            set { _light.shadowGenerator.Enabled = value; }
        }

        public LightPrefab(string name, LightType type, Scene scene)
            : base(name, scene)
        {
            SetLighType(type);
            _light.shadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
        }

        public void SetLighType(LightType type)
        {
            if (type == LightType.Directional)
                _light = AddComponent<DirectionalLight>();
            else if (type == LightType.Point)
                _light = AddComponent<PointLight>();
            else
                _light = AddComponent<Light>(); // TODO: Make it abstract !

            _type = type;
        }
    }
}
