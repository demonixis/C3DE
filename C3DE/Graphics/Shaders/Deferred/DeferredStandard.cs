using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredStandard : ShaderMaterial
    {
        private StandardMaterial _material;
        private Vector4 _features;

        public DeferredStandard(StandardMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Deferred/Standard");
        }

        public override void PrePassForward(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData, ref Vector4 fogData)
        {
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix)
        {
            _effect.Parameters["Projection"].SetValue(projectionMatrix);
            _effect.Parameters["View"].SetValue(viewMatrix);
            _effect.Parameters["EyePosition"].SetValue(cameraPosition);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            // Features
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.SpecularColor != null ? 1 : 0;
            _features.Z = _material.ReflectionMap != null ? 1 : 0;
            _features.W = _material.EmissiveMap != null ? 1 : 0;

            // Material
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["MainTexture"].SetValue(_material.MainTexture);
            _effect.Parameters["DiffuseColor"].SetValue(_material.DiffuseColor.ToVector3());
            _effect.Parameters["Features"].SetValue(_features);
            // Normal
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            // Specular
            _effect.Parameters["SpecularMap"].SetValue(_material.SpecularMap);
            _effect.Parameters["SpecularColor"].SetValue(_material.SpecularColor.ToVector3());
            _effect.Parameters["SpecularIntensity"].SetValue(_material.SpecularIntensity);
            _effect.Parameters["SpecularPower"].SetValue(_material.SpecularPower);
            // Emissive
            _effect.Parameters["EmissiveMap"].SetValue(_material.EmissiveMap);
            _effect.Parameters["EmissiveColor"].SetValue(_material.EmissiveColor.ToVector3());
            _effect.Parameters["EmissiveIntensity"].SetValue(_material.EmissiveIntensity);
            // Reflection
            _effect.Parameters["ReflectionIntensity"].SetValue(_material.ReflectionIntensity);
            _effect.Parameters["ReflectionMap"].SetValue(_material.ReflectionMap);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
