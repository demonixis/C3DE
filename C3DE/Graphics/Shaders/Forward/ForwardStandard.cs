using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardStandard : ForwardStandardBase
    {
        private StandardMaterial m_Material;
        protected EffectParameter m_EPNormalTexture;
        protected EffectParameter m_EPNormalTextureEnabled;
        protected EffectParameter m_EPReflectionTexture;
        protected EffectParameter m_EPReflectionTextureEnabled;
        protected EffectParameter m_EPEmissiveTextureEnabled;
        protected EffectParameter m_EPEmissiveTexture;
        protected EffectParameter m_EPEmissiveColor;
        protected EffectParameter m_EPEmissiveIntensity;

        public bool EmissiveEnabled => m_Material.EmissiveEnabled;

        public ForwardStandard(StandardMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Standard");
            SetupParamaters();
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();

            m_EPNormalTexture = _effect.Parameters["NormalTexture"];
            m_EPNormalTextureEnabled = _effect.Parameters["NormalTextureEnabled"];
            m_EPReflectionTexture = _effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = _effect.Parameters["ReflectionTextureEnabled"];
            m_EPEmissiveTextureEnabled = _effect.Parameters["EmissiveTextureEnabled"];
            m_EPEmissiveTexture = _effect.Parameters["EmissiveTexture"];
            m_EPEmissiveColor = _effect.Parameters["EmissiveColor"];
            m_EPEmissiveIntensity = _effect.Parameters["EmissiveIntensity"];
        }

        public override void Pass(Renderer renderable)
        {
            m_EPReflectionTexture.SetValue(m_Material.ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(m_Material.ReflectionTexture != null);
            BasePass(m_Material, renderable);
        }

        public override void LightPass(Renderer renderer, Light light)
        {
            m_EPEmissiveTextureEnabled.SetValue(m_Material.EmissiveTexture != null);
            m_EPEmissiveTexture.SetValue(m_Material.EmissiveTexture);
            m_EPEmissiveColor.SetValue(m_Material.EmissiveColor.ToVector3());
            m_EPEmissiveIntensity.SetValue(m_Material.EmissiveIntensity);
            m_EPNormalTexture.SetValue(m_Material.NormalMap);
            m_EPNormalTextureEnabled.SetValue(m_Material.NormalMap != null);
            BaseLightPass(m_Material, renderer, light);
        }
    }
}
