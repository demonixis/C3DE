using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandardWater : ForwardStandardBase
    {
        protected StandardWaterMaterial _material;
        protected EffectParameter _EPReflectionTexture;
        protected EffectParameter _EPReflectionTextureEnabled;
        protected EffectParameter _EPReflectionColor;
        protected EffectParameter _EPNormalMap;
        protected EffectParameter _EPNormalMapEnabled;
        protected EffectParameter _EPTotalTime;

        public ForwardStandardWater(StandardWaterMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/StandardWater");
            SetupParamaters();
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();

            _EPReflectionTexture = _effect.Parameters["ReflectionTexture"];
            _EPReflectionTextureEnabled = _effect.Parameters["ReflectionTextureEnabled"];
            _EPReflectionColor = _effect.Parameters["ReflectionColor"];
            _EPNormalMap = _effect.Parameters["NormalTexture"];
            _EPNormalMapEnabled = _effect.Parameters["NormalTextureEnabled"];
            _EPTotalTime = _effect.Parameters["TotalTime"];
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _EPTotalTime.SetValue(Time.TotalTime * _material.Speed);
            _EPNormalMap.SetValue(_material.NormalMap);
            _EPNormalMapEnabled.SetValue(_material.NormalMap != null);
            _EPReflectionTexture.SetValue(_material.ReflectionTexture);
            _EPReflectionTextureEnabled.SetValue(_material.ReflectionTexture != null);

            BasePass(_material, ref worldMatrix, receiveShadow);
        }
    }
}
