using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandardWater : ForwardStandardBase
    {
        protected StandardWaterMaterial _material;
        private Vector4 _features;

        public ForwardStandardWater(StandardWaterMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/StandardWater");
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.SpecularMap != null ? 1 : 0; 

            _effect.Parameters["TotalTime"].SetValue(Time.TotalTime * _material.Speed);
            BindCommonMaterialParameters(ref worldMatrix, receiveShadow, drawInstanced);
            BindStandardSurfaceParameters(
                _material.MainTexture,
                _material._diffuseColor,
                _material.SpecularColor.ToVector3(),
                _material.SpecularPower,
                _material.SpecularIntensity,
                _material.Tiling);
            BindStandardOptionalMaps(
                _features,
                _material.NormalMap,
                _material.SpecularMap,
                null,
                _material.ReflectionMap);
            BindEmissionAndReflection(Vector3.Zero, 0.0f, _material.ReflectionIntensity);
            _effect.Parameters["Alpha"].SetValue(_material.Alpha);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
