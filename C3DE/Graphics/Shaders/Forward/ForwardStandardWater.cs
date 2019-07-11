using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardStandardWater : ForwardStandardBase
    {
        protected StandardWaterMaterial m_Material;
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
            _effect = content.Load<Effect>("Shaders/Forward/StandardWater");
            SetupParamaters();
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();

            m_EPReflectionTexture = _effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = _effect.Parameters["ReflectionTextureEnabled"];
            m_EPReflectionColor = _effect.Parameters["ReflectionColor"];
            m_EPNormalMap = _effect.Parameters["NormalTexture"];
            m_EPNormalMapEnabled = _effect.Parameters["NormalTextureEnabled"];
            m_EPTotalTime = _effect.Parameters["TotalTime"];
        }

        public override void Pass(Renderer renderable)
        {
            m_EPTotalTime.SetValue(Time.TotalTime * m_Material.Speed);
            m_EPNormalMap.SetValue(m_Material.NormalMap);
            m_EPNormalMapEnabled.SetValue(m_Material.NormalMap != null);
            m_EPReflectionTexture.SetValue(m_Material.ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(m_Material.ReflectionTexture != null);
            m_EPReflectionColor.SetValue(m_Material.ReflectionColor.ToVector3());
            BasePass(m_Material, renderable);
        }

        public override void LightPass(Renderer renderer, Light light)
        {
            BaseLightPass(m_Material, renderer, light);
        }
    }
}
