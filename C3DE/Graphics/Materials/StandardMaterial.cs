using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using C3DE.Components.Lighting;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class StandardMaterial : StandardMaterialBase, IEmissiveMaterial
    {
        protected Vector3 m_EmissiveColor = Vector3.Zero;
        protected EffectPass m_PassEmissive;
        protected EffectParameter m_EPNormalTexture;
        protected EffectParameter m_EPNormalTextureEnabled;
        protected EffectParameter m_EPReflectionTexture;
        protected EffectParameter m_EPReflectionTextureEnabled;
        protected EffectParameter m_EPEmissiveTextureEnabled;
        protected EffectParameter m_EPEmissiveTexture;
        protected EffectParameter m_EPEmissiveColor;
        protected EffectParameter m_EPEmissiveIntensity;

        public TextureCube ReflectionTexture { get; set; }

        [DataMember]
        public Color EmissiveColor
        {
            get { return new Color(m_EmissiveColor); }
            set { m_EmissiveColor = value.ToVector3(); }
        }

        [DataMember]
        public bool EmissiveEnabled { get; set; } = false;

        [DataMember]
        public float EmissiveIntensity { get; set; } = 1.0f;
        
        public Texture2D EmissiveTexture { get; set; }

        public StandardMaterial(Scene scene, string name = "Standard Material")
            : base(scene)
        {
            Name = name;
        }

        protected override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/StandardEffect");
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();

            m_EPNormalTexture = m_Effect.Parameters["NormalTexture"];
            m_EPNormalTextureEnabled = m_Effect.Parameters["NormalTextureEnabled"];
            m_PassEmissive = m_Effect.CurrentTechnique.Passes["EmissivePass"];
            m_EPReflectionTexture = m_Effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = m_Effect.Parameters["ReflectionTextureEnabled"];
            m_EPEmissiveTextureEnabled = m_Effect.Parameters["EmissiveTextureEnabled"];
            m_EPEmissiveTexture = m_Effect.Parameters["EmissiveTexture"];
            m_EPEmissiveColor = m_Effect.Parameters["EmissiveColor"];
            m_EPEmissiveIntensity = m_Effect.Parameters["EmissiveIntensity"];
        }

        public override void Pass(Renderer renderable)
        {
            m_EPReflectionTexture.SetValue(ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(ReflectionTexture != null);
            base.Pass(renderable);
        }

        public override void LightPass(Renderer renderer, Light light)
        {
            m_EPNormalTexture.SetValue(NormalTexture);
            m_EPNormalTextureEnabled.SetValue(NormalTexture != null);
            base.LightPass(renderer, light);
        }

        public virtual void EmissivePass(Renderer renderer)
        {
            m_EPEmissiveTextureEnabled.SetValue(EmissiveTexture != null);
            m_EPEmissiveTexture.SetValue(EmissiveTexture);
            m_EPEmissiveColor.SetValue(m_EmissiveColor);
            m_EPEmissiveIntensity.SetValue(EmissiveIntensity);
            m_PassEmissive.Apply();
        }
    }
}
