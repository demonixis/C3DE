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
            m_Effect = content.Load<Effect>("Shaders/Deferred/Standard");

            m_PassLight = m_Effect.CurrentTechnique.Passes[0];
            m_EPView = m_Effect.Parameters["View"];
            m_EPProjection = m_Effect.Parameters["Projection"];
            m_EPEyePosition = m_Effect.Parameters["EyePosition"];
            m_EPAmbientColor = m_Effect.Parameters["AmbientColor"];
            m_EPWorld = m_Effect.Parameters["World"];
            m_EPTextureTilling = m_Effect.Parameters["TextureTiling"];
            m_EPDiffuseColor = m_Effect.Parameters["DiffuseColor"];
            m_EPMainTexture = m_Effect.Parameters["MainTexture"];
            m_EPSpecularLightColor = m_Effect.Parameters["SpecularLightColor"];
            m_EPSpecularPower = m_Effect.Parameters["SpecularPower"];
            m_EPSpecularIntensity = m_Effect.Parameters["SpecularIntensity"];
            m_EPSpecularTextureEnabled = m_Effect.Parameters["SpecularTextureEnabled"];
            m_EPSpecularTexture = m_Effect.Parameters["SpecularMap"];
            m_EPNormalTexture = m_Effect.Parameters["NormalMap"];
            m_EPNormalTextureEnabled = m_Effect.Parameters["NormalTextureEnabled"];
            m_EPReflectionTexture = m_Effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = m_Effect.Parameters["ReflectionTextureEnabled"];
            m_EPEmissiveTextureEnabled = m_Effect.Parameters["EmissiveTextureEnabled"];
            m_EPEmissiveTexture = m_Effect.Parameters["EmissiveTexture"];
            m_EPEmissiveColor = m_Effect.Parameters["EmissiveColor"];
            m_EPEmissiveIntensity = m_Effect.Parameters["EmissiveIntensity"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera.m_ViewMatrix);
            m_EPProjection.SetValue(camera.m_ProjectionMatrix);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
            m_EPAmbientColor.SetValue(Scene.current.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPEmissiveTextureEnabled.SetValue(m_Material.EmissiveTexture != null);
            m_EPEmissiveTexture.SetValue(m_Material.EmissiveTexture);
            m_EPEmissiveColor.SetValue(m_Material.EmissiveColor.ToVector3());
            m_EPEmissiveIntensity.SetValue(m_Material.EmissiveIntensity);
            m_EPReflectionTexture.SetValue(m_Material.ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(m_Material.ReflectionTexture != null);
            m_EPWorld.SetValue(renderable.Transform.m_WorldMatrix);
            m_EPTextureTilling.SetValue(m_Material.Tiling);
            m_EPDiffuseColor.SetValue(m_Material.m_DiffuseColor);
            m_EPMainTexture.SetValue(m_Material.MainTexture);
            m_EPSpecularLightColor.SetValue(m_Material.SpecularColor.ToVector3());
            m_EPSpecularPower.SetValue(m_Material.Shininess);
            m_EPSpecularIntensity.SetValue(m_Material.SpecularIntensity);
            m_EPSpecularTextureEnabled.SetValue(m_Material.SpecularTexture != null);
            m_EPSpecularTexture.SetValue(m_Material.SpecularTexture);
            m_EPNormalTexture.SetValue(m_Material.NormalTexture);
            m_EPNormalTextureEnabled.SetValue(m_Material.NormalTexture != null);
            m_PassLight.Apply();
        }
    }
}
