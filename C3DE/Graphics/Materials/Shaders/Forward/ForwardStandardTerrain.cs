using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardStandardTerrain : ForwardStandardBase
    {
        private StandardTerrainMaterial m_Material;
        private EffectParameter m_EPWeightTexture;
        private EffectParameter m_EPRockTexture;
        private EffectParameter m_EPSnowTexture;
        private EffectParameter m_EPSandTexture;

        public ForwardStandardTerrain(StandardTerrainMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Forward/StandardTerrain");
            SetupParamaters();
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();
            m_EPMainTexture = m_Effect.Parameters["MainTexture"];
            m_EPWeightTexture = m_Effect.Parameters["WeightMap"];
            m_EPRockTexture = m_Effect.Parameters["RockTexture"];
            m_EPSnowTexture = m_Effect.Parameters["SnowTexture"];
            m_EPSandTexture = m_Effect.Parameters["SandTexture"];
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
