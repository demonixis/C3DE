using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardStandardWater : ForwardStandardBase
    {
        private StandardWaterMaterial m_Material;
        protected EffectParameter m_EPReflectionTexture;
        protected EffectParameter m_EPReflectionTextureEnabled;
        protected EffectParameter m_EPReflectionColor;
        protected EffectParameter m_EPNormalMap;
        protected EffectParameter m_EPNormalMapEnabled;
        protected EffectParameter m_EPTotalTime;

        public ForwardStandardWater(StandardWaterMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Forward/StandardWater");
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();

            m_EPReflectionTexture = m_Effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = m_Effect.Parameters["ReflectionTextureEnabled"];
            m_EPReflectionColor = m_Effect.Parameters["ReflectionColor"];
            m_EPNormalMap = m_Effect.Parameters["NormalTexture"];
            m_EPNormalMapEnabled = m_Effect.Parameters["NormalTextureEnabled"];
            m_EPTotalTime = m_Effect.Parameters["TotalTime"];
        }

        public override void Pass(Renderer renderable)
        {
            PreparePass(m_Material, renderable);

            m_EPTotalTime.SetValue(Time.TotalTime * m_Material.Speed);
            m_EPNormalMap.SetValue(m_Material.NormalTexture);
            m_EPNormalMapEnabled.SetValue(m_Material.NormalTexture != null);
            m_EPReflectionTexture.SetValue(m_Material.ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(m_Material.ReflectionTexture != null);
            m_EPReflectionColor.SetValue(m_Material.ReflectionColor.ToVector3());
        }
    }
}
