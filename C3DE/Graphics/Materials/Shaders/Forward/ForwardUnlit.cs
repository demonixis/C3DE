using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardUnlit : ShaderMaterial
    {
        private UnlitMaterial m_Material;
        private EffectPass m_PassColor;
        private EffectPass m_PassTexture;
        private EffectParameter m_EPView;
        private EffectParameter m_EPProjection;
        private EffectParameter m_EPWorld;
        private EffectParameter m_EPTextureTilling;
        private EffectParameter m_EPDiffuseColor;
        private EffectParameter m_EPMainTexture;

        public ForwardUnlit(UnlitMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Unlit");
            SetupParamaters();
        }

        protected virtual void SetupParamaters()
        {
            m_PassColor = _effect.CurrentTechnique.Passes["UnlitColor"];
            m_PassTexture = _effect.CurrentTechnique.Passes["UnlitTexture"];
            m_EPView = _effect.Parameters["View"];
            m_EPProjection = _effect.Parameters["Projection"];
            m_EPWorld = _effect.Parameters["World"];
            m_EPTextureTilling = _effect.Parameters["TextureTilling"];
            m_EPDiffuseColor = _effect.Parameters["DiffuseColor"];
            m_EPMainTexture = _effect.Parameters["MainTexture"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera._viewMatrix);
            m_EPProjection.SetValue(camera._projectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPWorld.SetValue(renderable.Transform._worldMatrix);
            m_EPTextureTilling.SetValue(m_Material.Tiling);
            m_EPDiffuseColor.SetValue(m_Material._diffuseColor);
            m_EPMainTexture.SetValue(m_Material.MainTexture);

            if (m_Material.MainTexture == null)
                m_PassColor.Apply();
            else
                m_PassTexture.Apply();
        }
    }
}
