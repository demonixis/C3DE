using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandard : ForwardStandardBase
    {
        private StandardMaterial _material;
        private Vector4 _features;

        public ForwardStandard(StandardMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Standard");
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.EmissiveMap != null ? 1 : 0;
            _features.Z = _material.CutoutEnabled ? 1 : 0;
            _features.W = _material.SpecularTexture != null ? 1 : 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["SpecularMap"].SetValue(_material.SpecularTexture);
            _effect.Parameters["SpecularColor"].SetValue((float)_material.SpecularColor.R / 255.0f);
            _effect.Parameters["SpecularPower"].SetValue(_material.SpecularPower);
            _effect.Parameters["EmissiveMap"].SetValue(_material.EmissiveMap);
            _effect.Parameters["EmissiveColor"].SetValue(_material.EmissiveColor.ToVector3());
            _effect.Parameters["EmissiveIntensity"].SetValue(_material.EmissiveIntensity);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(receiveShadow);
            _effect.Parameters["DiffuseColor"].SetValue(_material._diffuseColor);
            _effect.Parameters["Cutout"].SetValue(_material.Cutout);
            _effect.Parameters["ReflectionIntensity"].SetValue(_material.ReflectionIntensity);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
