using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
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
            m_hasAlpha = true;
        }

        public override void LoadContent(ContentManager content)
        {
            if (ShaderQuality == ShaderQuality.Low)
                m_Effect = content.Load<Effect>("Shaders/WaterEffect.Low");
            else
                m_Effect = content.Load<Effect>("Shaders/WaterEffect");
        }

        public override void PrePass(Camera camera)
        {
            _totalTime += Time.DeltaTime / 10.0f;

            if (ShaderQuality == ShaderQuality.Normal)
            {
                m_Effect.Parameters["EyePosition"].SetValue(camera.Transform.LocalPosition);

                // Fog
                m_Effect.Parameters["FogColor"].SetValue(scene.RenderSettings.fogColor);
                m_Effect.Parameters["FogData"].SetValue(scene.RenderSettings.fogData);
            }

            m_Effect.Parameters["View"].SetValue(camera.view);
            m_Effect.Parameters["Projection"].SetValue(camera.projection);

            // Light
            if (scene.lights.Count > 0)
            {
                var light0 = scene.Lights[0];
                m_Effect.Parameters["LightColor"].SetValue(light0.color);
                m_Effect.Parameters["LightDirection"].SetValue(light0.transform.LocalRotation);
                m_Effect.Parameters["LightIntensity"].SetValue(light0.Intensity);
            }

            m_Effect.Parameters["AmbientColor"].SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            m_Effect.Parameters["TotalTime"].SetValue(_totalTime);
            m_Effect.Parameters["WaterTexture"].SetValue(MainTexture);

            if (ShaderQuality == ShaderQuality.Normal)
            {
                if (_normalMapEnabled)
                    m_Effect.Parameters["NormalTexture"].SetValue(NormalMap);

                if (_reflectiveMapEnabled)
                {
                    m_Effect.Parameters["ReflectiveTexture"].SetValue(ReflectiveMap);
                    m_Effect.Parameters["ReflectionColor"].SetValue(_reflectionColor);
                }

                m_Effect.Parameters["ReflectiveMapEnabled"].SetValue(_reflectiveMapEnabled);
                m_Effect.Parameters["NormalMapEnabled"].SetValue(_normalMapEnabled);
            }

            m_Effect.Parameters["TextureTiling"].SetValue(Tiling);
            m_Effect.Parameters["TextureOffset"].SetValue(Offset);
            m_Effect.Parameters["Alpha"].SetValue(WaterTransparency);
            m_Effect.Parameters["DiffuseColor"].SetValue(m_DiffuseColor);
            m_Effect.Parameters["SpecularColor"].SetValue(_specularColor);
            m_Effect.Parameters["Shininess"].SetValue(Shininess);
            m_Effect.Parameters["World"].SetValue(renderable.Transform.world);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
