using C3DE.Components.Lights;

namespace C3DE.Prefabs
{
    public class LightPrefab : GameObject
    {
        private Light _light;
        
        public Light Light
        {
            get { return _light; }
        }

        public LightType Type
        {
            get { return _light.TypeLight; }
            set { _light.TypeLight = value; }
        }

        public bool EnableShadows
        {
            get { return _light.shadowGenerator.Enabled; }
            set { _light.shadowGenerator.Enabled = value; }
        }

        public LightPrefab()
            : base()
        {
            Name = "LightPrefab-" + System.Guid.NewGuid();
            _light = AddComponent<Light>();
            _light.TypeLight = LightType.Directional;
            _light.shadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
        }

        public LightPrefab(string name, LightType type)
            : this()
        {
            Name = name;
        }
    }
}
