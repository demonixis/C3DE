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

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            BindCommonMaterialParameters(ref worldMatrix, receiveShadow, drawInstanced);
            BindStandardSurfaceParameters(
                _material.MainTexture,
                _material._diffuseColor,
                _material.SpecularColor.ToVector3(),
                _material.SpecularPower,
                _material.SpecularIntensity,
                _material.Tiling);

            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.EmissiveMap != null ? 1 : 0;
            _features.Z = _material.CutoutEnabled ? 1 : 0;
            _features.W = _material.SpecularMap != null ? 1 : 0;

            BindStandardOptionalMaps(
                _features,
                _material.NormalMap,
                _material.SpecularMap,
                _material.EmissiveMap,
                _material.ReflectionMap);
            BindEmissionAndReflection(
                _material.EmissiveColor.ToVector3(),
                _material.EmissiveIntensity,
                _material.ReflectionIntensity);

            _effect.Parameters["Cutout"]?.SetValue(_material.Cutout);

            _effect.Techniques[drawInstanced ? 1 : 0].Passes[0].Apply();
        }
    }
}
