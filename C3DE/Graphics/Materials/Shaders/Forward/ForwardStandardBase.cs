using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public abstract class ForwardStandardBase : ShaderMaterial, IMultipassLightingMaterial
    {
        protected EffectPass m_PassAmbient;
        protected EffectPass m_PassLight;
        protected EffectParameter m_EPView;
        protected EffectParameter m_EPProjection;
        protected EffectParameter m_EPEyePosition;
        protected EffectParameter m_EPAmbientColor;
        protected EffectParameter m_EPWorld;
        protected EffectParameter m_EPTextureTilling;
        protected EffectParameter m_EPDiffuseColor;
        protected EffectParameter m_EPMainTexture;
        protected EffectParameter m_EPSpecularLightColor;
        protected EffectParameter m_EPSpecularPower;
        protected EffectParameter m_EPSpecularIntensity;
        protected EffectParameter m_EPLightColor;
        protected EffectParameter m_EPLightDirection;
        protected EffectParameter m_EPLightPosition;
        protected EffectParameter m_EPLightSpotAngle;
        protected EffectParameter m_EPLightIntensity;
        protected EffectParameter m_EPLightRange;
        protected EffectParameter m_EPLightFallOff;
        protected EffectParameter m_EPLightType;
        protected EffectParameter m_EPShadowStrength;
        protected EffectParameter m_EPShadowBias;
        protected EffectParameter m_EPShadowMap;
        protected EffectParameter m_EPShadowEnabled;
        protected EffectParameter m_EPFogColor;
        protected EffectParameter m_EPFogData;
        protected EffectParameter m_EPLightView;
        protected EffectParameter m_EPLightProjection;
        protected EffectParameter m_EPSpecularTextureEnabled;
        protected EffectParameter m_EPSpecularTexture;

        protected virtual void SetupParamaters()
        {
            m_PassAmbient = _effect.CurrentTechnique.Passes["AmbientPass"];
            m_PassLight = _effect.CurrentTechnique.Passes["LightPass"];
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
            m_EPLightColor = _effect.Parameters["LightColor"];
            m_EPLightDirection = _effect.Parameters["LightDirection"];
            m_EPLightPosition = _effect.Parameters["LightPosition"];
            m_EPLightSpotAngle = _effect.Parameters["LightSpotAngle"];
            m_EPLightIntensity = _effect.Parameters["LightIntensity"];
            m_EPLightRange = _effect.Parameters["LightRange"];
            m_EPLightFallOff = _effect.Parameters["LightFallOff"];
            m_EPLightType = _effect.Parameters["LightType"];
            m_EPShadowStrength = _effect.Parameters["ShadowStrength"];
            m_EPShadowBias = _effect.Parameters["ShadowBias"];
            m_EPShadowMap = _effect.Parameters["ShadowMap"];
            m_EPShadowEnabled = _effect.Parameters["ShadowEnabled"];
            m_EPFogColor = _effect.Parameters["FogColor"];
            m_EPFogData = _effect.Parameters["FogData"];
            m_EPLightView = _effect.Parameters["LightView"];
            m_EPLightProjection = _effect.Parameters["LightProjection"];
            m_EPSpecularTextureEnabled = _effect.Parameters["SpecularTextureEnabled"];
            m_EPSpecularTexture = _effect.Parameters["SpecularTexture"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera._viewMatrix);
            m_EPProjection.SetValue(camera._projectionMatrix);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
            m_EPAmbientColor.SetValue(Scene.current.RenderSettings.ambientColor);
        }

        protected void BasePass(StandardMaterialBase material, Renderer renderable)
        {
            m_EPWorld.SetValue(renderable.Transform._worldMatrix);
            m_EPTextureTilling.SetValue(material.Tiling);
            m_EPDiffuseColor.SetValue(material._diffuseColor);
            m_EPMainTexture.SetValue(material.MainTexture);
            m_PassAmbient.Apply();
        }

        protected void BaseLightPass(StandardMaterialBase material, Renderer renderer, Light light)
        {
            m_EPSpecularLightColor.SetValue(material.SpecularColor.ToVector3());
            m_EPSpecularPower.SetValue(material.Shininess);
            m_EPSpecularIntensity.SetValue(material.SpecularIntensity);
            m_EPLightColor.SetValue(light._color);
            m_EPLightDirection.SetValue(light.Direction);
            m_EPLightPosition.SetValue(light._transform.LocalPosition);
            m_EPLightSpotAngle.SetValue(light.Angle);
            m_EPLightIntensity.SetValue(light.Intensity);
            m_EPLightRange.SetValue(light.Radius);
            m_EPLightFallOff.SetValue(light.FallOf);
            m_EPLightType.SetValue((int)light.TypeLight);

#if !DESKTOP && !ANDROID
            m_EPFogColor.SetValue(Scene.current.RenderSettings.fogColor);
            m_EPFogData.SetValue(Scene.current.RenderSettings.fogData);
#endif
            m_EPShadowStrength.SetValue(light._shadowGenerator.ShadowStrength);
            m_EPShadowBias.SetValue(light._shadowGenerator.ShadowBias);
            m_EPShadowMap.SetValue(light._shadowGenerator.ShadowMap);
            m_EPShadowEnabled.SetValue(renderer.ReceiveShadow);
            m_EPLightView.SetValue(light._viewMatrix);
            m_EPLightProjection.SetValue(light._projectionMatrix);
            m_EPSpecularTextureEnabled.SetValue(material.SpecularTexture != null);
            m_EPSpecularTexture.SetValue(material.SpecularTexture);
            m_PassLight.Apply();
        }

        public abstract void LightPass(Renderer renderer, Light light);
    }
}
