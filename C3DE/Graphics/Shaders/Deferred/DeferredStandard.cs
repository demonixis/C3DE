using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredStandard : ShaderMaterial
    {
        private StandardMaterial m_Material;
        protected EffectPass m_PassLight;
        protected EffectParameter m_EPView;
        protected EffectParameter m_EPProjection;
        protected EffectParameter m_EPEyePosition;
        protected EffectParameter m_EPAmbientColor;
        protected EffectParameter m_EPWorld;
        protected EffectParameter m_EPTextureTilling;
        protected EffectParameter m_EPDiffuseColor;
        protected EffectParameter m_EPMainTexture;
        protected EffectParameter m_EPNormalTexture;
        protected EffectParameter m_EPNormalTextureEnabled;
        protected EffectParameter m_EPReflectionTexture;
        protected EffectParameter m_EPReflectionTextureEnabled;
        protected EffectParameter m_EPSpecularLightColor;
        protected EffectParameter m_EPSpecularPower;
        protected EffectParameter m_EPSpecularIntensity;
        protected EffectParameter m_EPSpecularTextureEnabled;
        protected EffectParameter m_EPSpecularTexture;
        protected EffectParameter m_EPEmissiveTextureEnabled;
        protected EffectParameter m_EPEmissiveTexture;
        protected EffectParameter m_EPEmissiveColor;
        protected EffectParameter m_EPEmissiveIntensity;

        public DeferredStandard(StandardMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/Standard");

            m_PassLight = _effect.CurrentTechnique.Passes[0];
            m_EPView = _effect.Parameters["View"];
            m_EPProjection = _effect.Parameters["Projection"];
            m_EPEyePosition = _effect.Parameters["EyePosition"];
            m_EPAmbientColor = _effect.Parameters["AmbientColor"];
            m_EPWorld = _effect.Parameters["World"];
            m_EPTextureTilling = _effect.Parameters["TextureTiling"];
            m_EPDiffuseColor = _effect.Parameters["DiffuseColor"];
            m_EPMainTexture = _effect.Parameters["MainTexture"];
            m_EPSpecularLightColor = _effect.Parameters["SpecularLightColor"];
            m_EPSpecularPower = _effect.Parameters["SpecularPower"];
            m_EPSpecularIntensity = _effect.Parameters["SpecularIntensity"];
            m_EPSpecularTextureEnabled = _effect.Parameters["SpecularTextureEnabled"];
            m_EPSpecularTexture = _effect.Parameters["SpecularMap"];
            m_EPNormalTexture = _effect.Parameters["NormalMap"];
            m_EPNormalTextureEnabled = _effect.Parameters["NormalTextureEnabled"];
            m_EPReflectionTexture = _effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = _effect.Parameters["ReflectionTextureEnabled"];
            m_EPEmissiveTextureEnabled = _effect.Parameters["EmissiveTextureEnabled"];
            m_EPEmissiveTexture = _effect.Parameters["EmissiveTexture"];
            m_EPEmissiveColor = _effect.Parameters["EmissiveColor"];
            m_EPEmissiveIntensity = _effect.Parameters["EmissiveIntensity"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera._viewMatrix);
            m_EPProjection.SetValue(camera._projectionMatrix);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPEmissiveTextureEnabled.SetValue(m_Material.EmissiveMap != null);
            m_EPEmissiveTexture.SetValue(m_Material.EmissiveMap);
            m_EPEmissiveColor.SetValue(m_Material.EmissiveColor.ToVector3());
            m_EPEmissiveIntensity.SetValue(m_Material.EmissiveIntensity);
            m_EPReflectionTexture.SetValue(m_Material.ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(m_Material.ReflectionTexture != null);
            m_EPWorld.SetValue(renderable.Transform._worldMatrix);
            m_EPTextureTilling.SetValue(m_Material.Tiling);
            m_EPDiffuseColor.SetValue(m_Material._diffuseColor);
            m_EPMainTexture.SetValue(m_Material.MainTexture);
            m_EPSpecularLightColor.SetValue(m_Material.SpecularColor.ToVector3());
            m_EPSpecularPower.SetValue(m_Material.SpecularPower);
            m_EPSpecularIntensity.SetValue(m_Material.SpecularIntensity);
            m_EPSpecularTextureEnabled.SetValue(m_Material.SpecularTexture != null);
            m_EPSpecularTexture.SetValue(m_Material.SpecularTexture);
            m_EPNormalTexture.SetValue(m_Material.NormalMap);
            m_EPNormalTextureEnabled.SetValue(m_Material.NormalMap != null);
            m_PassLight.Apply();
        }
    }
}
