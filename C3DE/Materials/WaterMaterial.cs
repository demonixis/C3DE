using C3DE.Components;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Materials
{
    [DataContract]
    public class WaterMaterial : Material
    {
        private Texture2D _normalMap;
        private TextureCube _reflectiveMap;
        private bool _normalMapEnabled;
        private bool _reflectiveMapEnabled;
        private Vector3 _reflectionColor;
        private Vector3 _specularColor;
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

        [DataMember]
        public Color ReflectionColor
        {
            get { return new Color(_reflectionColor); }
            set { _reflectionColor = value.ToVector3(); }
        }

        [DataMember]
        public Color SpecularColor
        {
            get { return new Color(_specularColor); }
            set { _specularColor = value.ToVector3(); }
        }

        [DataMember]
        public float WaterTransparency { get; set; }

        [DataMember]
        public float Shininess { get; set; }

        public WaterMaterial(Scene scene, string name = "Water Material")
            : base(scene)
        {
            WaterTransparency = 0.45f;
            DiffuseColor = Color.White;
            Shininess = 250.0f;
            _specularColor = new Vector3(0.6f, 0.6f, 0.6f);
            _reflectionColor = new Vector3(1.0f);
            _normalMapEnabled = false;
            _reflectiveMapEnabled = false;
            _totalTime = 0.0f;
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            if (ShaderQuality == ShaderQuality.Low)
                effect = content.Load<Effect>("FX/WaterEffect.Low");
            else
                effect = content.Load<Effect>("FX/WaterEffect");
        }

        public override void PrePass(Camera camera)
        {
            _totalTime += Time.DeltaTime / 10.0f;

            if (ShaderQuality == ShaderQuality.Normal)
            {
                effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);

                // Fog
                effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
                effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);
            }

            effect.Parameters["View"].SetValue(camera.view);
            effect.Parameters["Projection"].SetValue(camera.projection);

            // Light
            if (scene.lights.Count > 0)
            {
                var light0 = scene.Lights[0];
                effect.Parameters["LightColor"].SetValue(light0.diffuseColor);
                effect.Parameters["LightDirection"].SetValue(light0.Direction);
                effect.Parameters["LightIntensity"].SetValue(light0.Intensity);
            }

            effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            effect.Parameters["TotalTime"].SetValue(_totalTime);
            effect.Parameters["WaterTexture"].SetValue(diffuseTexture);

            if (ShaderQuality == ShaderQuality.Normal)
            {
                if (_normalMapEnabled)
                    effect.Parameters["NormalTexture"].SetValue(NormalMap);

                if (_reflectiveMapEnabled)
                {
                    effect.Parameters["ReflectiveTexture"].SetValue(ReflectiveMap);
                    effect.Parameters["ReflectionColor"].SetValue(_reflectionColor);
                }

                effect.Parameters["ReflectiveMapEnabled"].SetValue(_reflectiveMapEnabled);
                effect.Parameters["NormalMapEnabled"].SetValue(_normalMapEnabled);
            }

            effect.Parameters["TextureTiling"].SetValue(Tiling);
            effect.Parameters["TextureOffset"].SetValue(Offset);
            effect.Parameters["Alpha"].SetValue(WaterTransparency);
            effect.Parameters["DiffuseColor"].SetValue(diffuseColor);
            effect.Parameters["SpecularColor"].SetValue(_specularColor);
            effect.Parameters["Shininess"].SetValue(Shininess);
            effect.Parameters["World"].SetValue(renderable.Transform.world);
            effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
