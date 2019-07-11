using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardStandardTerrain : ForwardStandardBase
    {
        protected StandardTerrainMaterial _material;
        protected EffectParameter m_EPWeightTexture;
        protected EffectParameter m_EPRockTexture;
        protected EffectParameter m_EPSnowTexture;
        protected EffectParameter m_EPSandTexture;
        protected EffectParameter m_EPGrassNormalTexture;
        protected EffectParameter m_EPRockNormalTexture;
        protected EffectParameter m_EPSnowNormalTexture;
        protected EffectParameter m_EPSandNormalTexture;
        protected EffectParameter m_EPNormalMapEnabled;

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
            m_EPMainTexture = _effect.Parameters["MainTexture"];
            m_EPWeightTexture = _effect.Parameters["WeightMap"];
            m_EPRockTexture = _effect.Parameters["RockTexture"];
            m_EPSnowTexture = _effect.Parameters["SnowTexture"];
            m_EPSandTexture = _effect.Parameters["SandTexture"];
            // Normals
            m_EPGrassNormalTexture = _effect.Parameters["NormalMap"];
            m_EPRockNormalTexture = _effect.Parameters["RockNormalMap"];
            m_EPSnowNormalTexture = _effect.Parameters["SnowNormalMap"];
            m_EPSandNormalTexture = _effect.Parameters["SandNormalMap"];
            m_EPNormalMapEnabled = _effect.Parameters["NormalMapEnabled"];
        }

        public override void Pass(Renderer renderable)
        {
            m_EPWeightTexture.SetValue(_material.WeightTexture);
            m_EPSnowTexture.SetValue(_material.SnowTexture);
            m_EPSandTexture.SetValue(_material.SandTexture);
            m_EPRockTexture.SetValue(_material.RockTexture);

#if WINDOWS
            var normalMapEnabled = _material.NormalMap != null &&
                _material.RockNormalMap != null &&
                _material.SandNormalMap != null &&
                _material.SnownNormalMap != null;

            m_EPNormalMapEnabled.SetValue(normalMapEnabled ? 1 : 0);

            if (normalMapEnabled)
            {
                m_EPGrassNormalTexture.SetValue(_material.NormalMap);
                m_EPRockNormalTexture.SetValue(_material.RockNormalMap);
                m_EPSnowNormalTexture.SetValue(_material.SnownNormalMap);
                m_EPSandNormalTexture.SetValue(_material.SandNormalMap);
            }
#endif

            BasePass(_material, renderable);
        }

        public override void LightPass(Renderer renderer, Light light)
        {
            BaseLightPass(_material, renderer, light);
        }
    }
}
