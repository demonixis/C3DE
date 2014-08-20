using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public class WaterMaterial : Material
    {
        private Texture2D _normalMap;
        private TextureCube _reflectiveMap;
        private bool _normalMapEnabled;
        private bool _reflectiveMapEnabled;
        private Vector4 _reflectionColor;
        private Vector4 _specularColor;
        private float _totalTime;

        public TextureCube ReflectiveMap
        {
            get { return _reflectiveMap; }
            set
            {
                _reflectiveMap = value;
                _reflectiveMapEnabled = (value != null);
            }
        }

        public Texture2D NormalMap 
        {
            get { return _normalMap; }
            set 
            {
                _normalMap = value;
                _normalMapEnabled = (value != null);
            }
        }

        public Color ReflectionColor
        {
            get { return new Color(_reflectionColor); }
            set { _reflectionColor = value.ToVector4(); }
        }

        public Color SpecularColor
        {
            get { return new Color(_specularColor); }
            set { _specularColor = value.ToVector4(); }
        }

        public float WaterTransparency { get; set; }
        public float Shininess { get; set; }

        public WaterMaterial(Scene scene)
            : base(scene)
        {
            WaterTransparency = 0.45f;
            DiffuseColor = Color.White;
            Shininess = 250.0f;
            _specularColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
            _reflectionColor = new Vector4(1.0f);
            _normalMapEnabled = false;
            _reflectiveMapEnabled = false;
            _totalTime = 0.0f;
        }

        public override void LoadContent(ContentManager content)
        {
            effect = content.Load<Effect>("FX/WaterEffect");
        }

        public override void PrePass()
        {
            _totalTime += Time.DeltaTime / 10.0f;

            effect.Parameters["View"].SetValue(scene.MainCamera.view);
            effect.Parameters["Projection"].SetValue(scene.MainCamera.projection);
            effect.Parameters["EyePosition"].SetValue(scene.MainCamera.SceneObject.Transform.Position);

            // Light
            var light0 = scene.Lights[0]; // FIXME
            effect.Parameters["LightColor"].SetValue(light0.diffuseColor);
            effect.Parameters["LightDirection"].SetValue(light0.Direction);
            effect.Parameters["LightIntensity"].SetValue(light0.Intensity);
            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);

            // Fog
            effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
            effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);
        }

        public override void Pass(RenderableComponent renderable)
        {
            effect.Parameters["ReflectiveMapEnabled"].SetValue(_reflectiveMapEnabled);
            effect.Parameters["NormalMapEnabled"].SetValue(_normalMapEnabled);

            effect.Parameters["TotalTime"].SetValue(_totalTime);
            effect.Parameters["WaterTexture"].SetValue(mainTexture);

            if (_normalMapEnabled)
                effect.Parameters["NormalTexture"].SetValue(NormalMap);

            if (_reflectiveMapEnabled)
            {
                effect.Parameters["ReflectiveTexture"].SetValue(ReflectiveMap);
                effect.Parameters["ReflectionColor"].SetValue(_reflectionColor);
            }

            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["Alpha"].SetValue(WaterTransparency);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["Shininess"].SetValue(Shininess);
            effect.Parameters["World"].SetValue(renderable.SceneObject.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
