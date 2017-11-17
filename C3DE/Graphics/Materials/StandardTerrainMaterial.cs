using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class StandardTerrainMaterial : StandardMaterialBase
    {
        private EffectParameter m_EPWeightTexture;
        private EffectParameter m_EPRockTexture;
        private EffectParameter m_EPSnowTexture;
        private EffectParameter m_EPSandTexture;

        public Texture2D SnowTexture { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D WeightTexture { get; set; }

        public StandardTerrainMaterial(Scene scene, string name = "Standard Terrain Material")
            : base(scene, name)
        {
        }

        protected override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/StandardTerrainEffect");
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
            m_EPWeightTexture.SetValue(WeightTexture);
            m_EPSnowTexture.SetValue(SnowTexture);
            m_EPSandTexture.SetValue(SandTexture);
            m_EPRockTexture.SetValue(RockTexture);
            base.Pass(renderable);
        }
    }
}
