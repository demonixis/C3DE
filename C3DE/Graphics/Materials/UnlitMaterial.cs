using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class UnlitMaterial : Material
    {
        private EffectPass m_PassColor;
        private EffectPass m_PassTexture;

        private EffectParameter m_EPView;
        private EffectParameter m_EPProjection;
        private EffectParameter m_EPWorld;
        private EffectParameter m_EPTextureTilling;
        private EffectParameter m_EPDiffuseColor;
        private EffectParameter m_EPMainTexture;

        public UnlitMaterial()
            : this(null)
        {
        }

        public UnlitMaterial(Scene scene, string name = "Unlit Material")
            : base(scene)
        {
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Forward/Unlit");
            m_PassColor = m_Effect.CurrentTechnique.Passes["UnlitColor"];
            m_PassTexture = m_Effect.CurrentTechnique.Passes["UnlitTexture"];
            m_EPView = m_Effect.Parameters["View"];
            m_EPProjection = m_Effect.Parameters["Projection"];
            m_EPWorld = m_Effect.Parameters["World"];
            m_EPTextureTilling = m_Effect.Parameters["TextureTilling"];
            m_EPDiffuseColor = m_Effect.Parameters["DiffuseColor"];
            m_EPMainTexture = m_Effect.Parameters["MainTexture"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera.m_ViewMatrix);
            m_EPProjection.SetValue(camera.m_ProjectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPWorld.SetValue(renderable.Transform.m_WorldMatrix);
            m_EPTextureTilling.SetValue(Tiling);
            m_EPDiffuseColor.SetValue(m_DiffuseColor);
            m_EPMainTexture.SetValue(MainTexture);

            if (MainTexture == null)
                m_PassColor.Apply();
            else
                m_PassTexture.Apply();
        }
    }
}
