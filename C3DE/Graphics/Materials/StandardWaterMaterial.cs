using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class StandardWaterMaterial : StandardMaterialBase
    {
        protected Vector3 _reflectionColor = Vector3.One;
        protected EffectParameter m_EPReflectionTexture;
        protected EffectParameter m_EPReflectionTextureEnabled;
        protected EffectParameter m_EPReflectionColor;
        protected EffectParameter m_EPNormalMap;
        protected EffectParameter m_EPNormalMapEnabled;
        protected EffectParameter m_EPTotalTime;

        public Texture2D NormalMap { get; set; }

        public TextureCube ReflectionTexture { get; set; }

        public float Speed = 0.5f;

        [DataMember]
        public float ReflectionIntensity { get; set; } = 0.35f;

        [DataMember]
        public Color ReflectionColor
        {
            get => new Color(_reflectionColor);
            set { _reflectionColor = value.ToVector3(); }
        }

        public StandardWaterMaterial(Scene scene, string name = "Water Material")
            : base(scene, name)
        {
            m_hasAlpha = true;
        }

        protected override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Forward/StandardWater");
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();

            m_EPReflectionTexture = m_Effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = m_Effect.Parameters["ReflectionTextureEnabled"];
            m_EPReflectionColor = m_Effect.Parameters["ReflectionColor"];
            m_EPNormalMap = m_Effect.Parameters["NormalTexture"];
            m_EPNormalMapEnabled = m_Effect.Parameters["NormalTextureEnabled"];
            m_EPTotalTime = m_Effect.Parameters["TotalTime"];
        }

        public override void Pass(Renderer renderable)
        {
            m_EPTotalTime.SetValue(Time.TotalTime * Speed);
            m_EPNormalMap.SetValue(NormalMap);
            m_EPNormalMapEnabled.SetValue(NormalMap != null);
            m_EPReflectionTexture.SetValue(ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(ReflectionTexture != null);
            m_EPReflectionColor.SetValue(_reflectionColor);

            base.Pass(renderable);
        }
    }
}
