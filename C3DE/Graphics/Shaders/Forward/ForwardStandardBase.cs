using C3DE.Components.Lighting;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public abstract class ForwardStandardBase : ForwardShader
    {
        protected EffectParameter _EPView;
        protected EffectParameter _EPProjection;
        protected EffectParameter _EPEyePosition;
        protected EffectParameter _EPAmbientColor;
        protected EffectParameter _EPWorld;
        protected EffectParameter _EPTextureTilling;
        protected EffectParameter _EPDiffuseColor;
        protected EffectParameter _EPMainTexture;
        protected EffectParameter _EPSpecularLightColor;
        protected EffectParameter _EPSpecularPower;
        protected EffectParameter _EPSpecularIntensity;
        protected EffectParameter _EPLightColor;
        protected EffectParameter _EPLightDirection;
        protected EffectParameter _EPLightPosition;
        protected EffectParameter _EPLightSpotAngle;
        protected EffectParameter _EPLightIntensity;
        protected EffectParameter _EPLightRange;
        protected EffectParameter _EPLightFallOff;
        protected EffectParameter _EPLightType;
        protected EffectParameter _EPShadowStrength;
        protected EffectParameter _EPShadowBias;
        protected EffectParameter _EPShadowMap;
        protected EffectParameter _EPShadowEnabled;
        protected EffectParameter _EPFogColor;
        protected EffectParameter _EPFogData;
        protected EffectParameter _EPLightView;
        protected EffectParameter _EPLightProjection;
        protected EffectParameter _EPSpecularTextureEnabled;
        protected EffectParameter _EPSpecularTexture;

        protected virtual void SetupParamaters()
        {
            _EPView = _effect.Parameters["View"];
            _EPProjection = _effect.Parameters["Projection"];
            _EPEyePosition = _effect.Parameters["EyePosition"];
            _EPAmbientColor = _effect.Parameters["AmbientColor"];
            _EPWorld = _effect.Parameters["World"];
            _EPTextureTilling = _effect.Parameters["TextureTiling"];
            _EPDiffuseColor = _effect.Parameters["DiffuseColor"];
            _EPMainTexture = _effect.Parameters["MainTexture"];
            _EPSpecularLightColor = _effect.Parameters["SpecularLightColor"];
            _EPSpecularPower = _effect.Parameters["SpecularPower"];
            _EPSpecularIntensity = _effect.Parameters["SpecularIntensity"];
            _EPLightColor = _effect.Parameters["LightColor"];
            _EPLightDirection = _effect.Parameters["LightDirection"];
            _EPLightPosition = _effect.Parameters["LightPosition"];
            _EPLightSpotAngle = _effect.Parameters["LightSpotAngle"];
            _EPLightIntensity = _effect.Parameters["LightIntensity"];
            _EPLightRange = _effect.Parameters["LightRange"];
            _EPLightFallOff = _effect.Parameters["LightFallOff"];
            _EPLightType = _effect.Parameters["LightType"];
            _EPShadowStrength = _effect.Parameters["ShadowStrength"];
            _EPShadowBias = _effect.Parameters["ShadowBias"];
            _EPShadowMap = _effect.Parameters["ShadowMap"];
            _EPShadowEnabled = _effect.Parameters["ShadowEnabled"];
            _EPFogColor = _effect.Parameters["FogColor"];
            _EPFogData = _effect.Parameters["FogData"];
            _EPLightView = _effect.Parameters["LightView"];
            _EPLightProjection = _effect.Parameters["LightProjection"];
            _EPSpecularTextureEnabled = _effect.Parameters["SpecularTextureEnabled"];
            _EPSpecularTexture = _effect.Parameters["SpecularTexture"];
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData)
        {
            _EPView.SetValue(viewMatrix);
            _EPProjection.SetValue(projectionMatrix);
            _EPEyePosition.SetValue(cameraPosition);
            _EPAmbientColor.SetValue(Scene.current.RenderSettings.ambientColor);
        }

        protected void BasePass(StandardMaterialBase material, ref Matrix worldMatrix, bool receiveShadow)
        {
            _EPWorld.SetValue(worldMatrix);
            _EPTextureTilling.SetValue(material.Tiling);
            _EPDiffuseColor.SetValue(material._diffuseColor);
            _EPMainTexture.SetValue(material.MainTexture);

            // FIXME: Will be removed when singlepass lighting will be done.
            var lights = Scene.current.lights;
            LegacyLightPass(material, receiveShadow, lights.Count > 0 ? lights[0] : null);

            _effect.CurrentTechnique.Passes[0].Apply();
        }

        protected void LegacyLightPass(StandardMaterialBase material, bool receiveShadow, Light light)
        {
            if (light == null)
                return;

            _EPSpecularLightColor.SetValue(material.SpecularColor.ToVector3());
            _EPSpecularPower.SetValue(material.SpecularPower);
            _EPSpecularIntensity.SetValue(material.SpecularIntensity);
            _EPLightColor.SetValue(light._color);
            _EPLightDirection.SetValue(light.Direction);
            _EPLightPosition.SetValue(light._transform.LocalPosition);
            _EPLightSpotAngle.SetValue(light.Angle);
            _EPLightIntensity.SetValue(light.Intensity);
            _EPLightRange.SetValue(light.Radius);
            _EPLightFallOff.SetValue(light.FallOf);
            _EPLightType.SetValue((int)light.TypeLight);

#if !DESKTOP && !ANDROID
            _EPFogColor.SetValue(Scene.current.RenderSettings.fogColor);
            _EPFogData.SetValue(Scene.current.RenderSettings.fogData);
#endif
            _EPShadowStrength.SetValue(light._shadowGenerator.ShadowStrength);
            _EPShadowBias.SetValue(light._shadowGenerator.ShadowBias);
            _EPShadowMap.SetValue(light._shadowGenerator.ShadowMap);
            _EPShadowEnabled.SetValue(receiveShadow);
            _EPLightView.SetValue(light._viewMatrix);
            _EPLightProjection.SetValue(light._projectionMatrix);
            _EPSpecularTextureEnabled.SetValue(material.SpecularTexture != null);
            _EPSpecularTexture.SetValue(material.SpecularTexture);
        }
    }
}
