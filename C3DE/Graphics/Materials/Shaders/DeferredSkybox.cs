using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class DeferredSkybox : ShaderMaterial
    {
        private Skybox m_Skybox;
        private EffectPass m_DefaultPass;
        protected EffectParameter m_EPWorld;
        protected EffectParameter m_EPView;
        protected EffectParameter m_EPProjection;
        protected EffectParameter m_EPMainTexture;
        protected EffectParameter m_EPEyePosition;
        protected EffectParameter m_EPFogEnabled;
        protected EffectParameter m_EPFogColor;
        protected EffectParameter m_EPFogData;

        public override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/Deferred/Skybox");
        }

        public override void Pass(Renderer renderable)
        {
        }

        public override void PrePass(Camera camera)
        {
            m_Effect.Parameters["World"].SetValue(m_Skybox.WorldMatrix);
            m_Effect.Parameters["Projection"].SetValue(camera.m_ProjectionMatrix);
            m_Effect.Parameters["View"].SetValue(camera.m_ViewMatrix);
            m_Effect.Parameters["Texture"].SetValue(m_Skybox.Texture);
            m_Effect.Parameters["EyePosition"].SetValue(camera.Transform.Position);
            m_Effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
