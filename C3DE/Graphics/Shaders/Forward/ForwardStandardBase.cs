using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public abstract class ForwardStandardBase : ShaderMaterial
    {
        private static readonly Vector4 DisabledFeatures = Vector4.Zero;

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
            _effect.Parameters["AmbientColor"].SetValue(lightData.Ambient);
            _effect.Parameters["LightCount"].SetValue(lightData.Count);

            if (lightData.Count > 0)
            {
                _effect.Parameters["LightPosition"].SetValue(lightData.Positions);
                _effect.Parameters["LightColor"].SetValue(lightData.Colors);
                _effect.Parameters["LightData"].SetValue(lightData.Data);
                _effect.Parameters["SpotData"]?.SetValue(lightData.SpotData);
            }

            _effect.Parameters["ShadowStrength"]?.SetValue(shadowData.Data.Z);
            _effect.Parameters["ShadowBias"]?.SetValue(shadowData.Data.Y);
            _effect.Parameters["ShadowMap"]?.SetValue(shadowData.ShadowMap);
            _effect.Parameters["LightView"]?.SetValue(shadowData.ViewMatrix);
            _effect.Parameters["LightProjection"]?.SetValue(shadowData.ProjectionMatrix);
            _effect.Parameters["FogData"]?.SetValue(fogData);
        }

        protected void BindCommonMaterialParameters(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _effect.Parameters["World"].SetValue(drawInstanced ? Matrix.Identity : worldMatrix);
            _effect.Parameters["ShadowEnabled"]?.SetValue(receiveShadow);
        }

        protected void BindStandardSurfaceParameters(
            Texture2D albedoMap,
            Vector3 diffuseColor,
            Vector3 specularColor,
            int specularPower,
            float specularIntensity,
            Vector2 textureTiling)
        {
            _effect.Parameters["AlbedoMap"]?.SetValue(albedoMap);
            _effect.Parameters["DiffuseColor"]?.SetValue(diffuseColor);
            _effect.Parameters["SpecularColor"]?.SetValue(specularColor);
            _effect.Parameters["SpecularPower"]?.SetValue(specularPower);
            _effect.Parameters["SpecularIntensity"]?.SetValue(specularIntensity);
            _effect.Parameters["TextureTiling"]?.SetValue(textureTiling);
        }

        protected void BindStandardOptionalMaps(
            Vector4? features = null,
            Texture2D normalMap = null,
            Texture2D specularMap = null,
            Texture2D emissiveMap = null,
            TextureCube reflectionMap = null)
        {
            _effect.Parameters["Features"]?.SetValue(features ?? DisabledFeatures);
            _effect.Parameters["NormalMap"]?.SetValue(normalMap);
            _effect.Parameters["SpecularMap"]?.SetValue(specularMap);
            _effect.Parameters["EmissiveMap"]?.SetValue(emissiveMap);
            _effect.Parameters["ReflectionMap"]?.SetValue(reflectionMap);
        }

        protected void BindEmissionAndReflection(
            Vector3 emissiveColor,
            float emissiveIntensity,
            float reflectionIntensity)
        {
            _effect.Parameters["EmissiveColor"]?.SetValue(emissiveColor);
            _effect.Parameters["EmissiveIntensity"]?.SetValue(emissiveIntensity);
            _effect.Parameters["ReflectionIntensity"]?.SetValue(reflectionIntensity);
        }
    }
}
