using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardStandardTerrain : ForwardStandardBase
    {
        protected StandardTerrainMaterial m_Material;
        protected EffectParameter m_EPWeightTexture;
        protected EffectParameter m_EPRockTexture;
        protected EffectParameter m_EPSnowTexture;
        protected EffectParameter m_EPSandTexture;

        public ForwardStandardTerrain(StandardTerrainMaterial material)
        {
            m_Material = material;
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
        }

        public override void Pass(Renderer renderable)
        {
            m_EPWeightTexture.SetValue(m_Material.WeightTexture);
            m_EPSnowTexture.SetValue(m_Material.SnowTexture);
            m_EPSandTexture.SetValue(m_Material.SandTexture);
            m_EPRockTexture.SetValue(m_Material.RockTexture);
            BasePass(m_Material, renderable);
        }

        public override void LightPass(Renderer renderer, Light light)
        {
            BaseLightPass(m_Material, renderer, light);
        }
    }
}
