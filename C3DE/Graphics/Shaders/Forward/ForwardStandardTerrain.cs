using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandardTerrain : ForwardStandardBase
    {
        protected StandardTerrainMaterial _material;
        protected EffectParameter _EPWeightTexture;
        protected EffectParameter _EPRockTexture;
        protected EffectParameter _EPSnowTexture;
        protected EffectParameter _EPSandTexture;
        protected EffectParameter _EPGrassNormalTexture;
        protected EffectParameter _EPRockNormalTexture;
        protected EffectParameter _EPSnowNormalTexture;
        protected EffectParameter _EPSandNormalTexture;
        protected EffectParameter _EPNormalMapEnabled;

        public ForwardStandardTerrain(StandardTerrainMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/StandardTerrain");
            SetupParamaters();
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();
            _EPMainTexture = _effect.Parameters["MainTexture"];
            _EPWeightTexture = _effect.Parameters["WeightMap"];
            _EPRockTexture = _effect.Parameters["RockTexture"];
            _EPSnowTexture = _effect.Parameters["SnowTexture"];
            _EPSandTexture = _effect.Parameters["SandTexture"];
            // Normals
            _EPGrassNormalTexture = _effect.Parameters["GrassNormalMap"];
            _EPRockNormalTexture = _effect.Parameters["RockNormalMap"];
            _EPSnowNormalTexture = _effect.Parameters["SnowNormalMap"];
            _EPSandNormalTexture = _effect.Parameters["SandNormalMap"];
            _EPNormalMapEnabled = _effect.Parameters["NormalMapEnabled"];
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _EPWeightTexture.SetValue(_material.WeightTexture);
            _EPSnowTexture.SetValue(_material.SnowTexture);
            _EPSandTexture.SetValue(_material.SandTexture);
            _EPRockTexture.SetValue(_material.RockTexture);

#if WINDOWS
            var normalMapEnabled = _material.NormalMap != null &&
                _material.RockNormalMap != null &&
                _material.SandNormalMap != null &&
                _material.SnownNormalMap != null;

            _EPNormalMapEnabled.SetValue(normalMapEnabled ? 1 : 0);

            if (normalMapEnabled)
            {
                _EPGrassNormalTexture.SetValue(_material.NormalMap);
                _EPRockNormalTexture.SetValue(_material.RockNormalMap);
                _EPSnowNormalTexture.SetValue(_material.SnownNormalMap);
                _EPSandNormalTexture.SetValue(_material.SandNormalMap);
            }
#endif

            BasePass(_material, ref worldMatrix, receiveShadow);
        }
    }
}
