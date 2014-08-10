using C3DE.Components.Lights;

namespace C3DE.Prefabs
{
    public class LightPrefab : Prefab
    {
        private Light _light;
        
        public Light Light
        {
            get { return _light; }
        }

        public LightType Type
        {
            get { return _light.Type; }
            set { _light.Type = value; }
        }

        public bool EnableShadows
        {
            get { return _light.shadowGenerator.Enabled; }
            set { _light.shadowGenerator.Enabled = value; }
        }

        public LightPrefab(string name, LightType type, Scene scene)
            : base(name, scene)
        {
            _light = AddComponent<Light>();
            _light.Type = type;
            _light.shadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
        }
    }
}
