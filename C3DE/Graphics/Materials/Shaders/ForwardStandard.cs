using System;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardStandard : ForwardStandardBase, IMultipassLightingMaterial, IEmissiveMaterial
    {
        private StandardMaterial m_Material;
        protected EffectPass m_PassEmissive;
        protected EffectParameter m_EPNormalTexture;
        protected EffectParameter m_EPNormalTextureEnabled;
        protected EffectParameter m_EPReflectionTexture;
        protected EffectParameter m_EPReflectionTextureEnabled;
        protected EffectParameter m_EPEmissiveTextureEnabled;
        protected EffectParameter m_EPEmissiveTexture;
        protected EffectParameter m_EPEmissiveColor;
        protected EffectParameter m_EPEmissiveIntensity;

        public bool EmissiveEnabled => throw new NotImplementedException();

        public ForwardStandard(StandardMaterial material)
        {
           
        }

        public override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Forward/Standard");
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
            PreparePass(m_Material, renderable);
        }

        public void EmissivePass(Renderer renderer)
        {
            throw new NotImplementedException();
        }

        public void LightPass(Renderer renderer, Light light)
        {
            throw new NotImplementedException();
        }
    }
}
