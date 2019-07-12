using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
   public class ForwardSkybox : ForwardShader
    {
        private Skybox m_Skybox;
        protected EffectParameter m_EPWorld;
        protected EffectParameter m_EPView;
        protected EffectParameter m_EPProjection;
        protected EffectParameter m_EPMainTexture;
        protected EffectParameter m_EPEyePosition;
        protected EffectParameter m_EPFogEnabled;
        protected EffectParameter m_EPFogColor;
        protected EffectParameter m_EPFogData;

        public ForwardSkybox(Skybox skybox)
        {
            m_Skybox = skybox;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Skybox");
            m_EPView = _effect.Parameters["View"];
            m_EPProjection = _effect.Parameters["Projection"];
            m_EPMainTexture = _effect.Parameters["MainTexture"];
            m_EPEyePosition = _effect.Parameters["EyePosition"];
            m_EPWorld = _effect.Parameters["World"];
            m_EPFogEnabled = _effect.Parameters["FogEnabled"];
            m_EPFogColor = _effect.Parameters["FogColor"];
            m_EPFogData = _effect.Parameters["FogData"];
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData)
        {
            m_EPView.SetValue(viewMatrix);
            m_EPProjection.SetValue(projectionMatrix);
            m_EPEyePosition.SetValue(cameraPosition);
            m_EPMainTexture.SetValue(m_Skybox.Texture);
            m_EPWorld.SetValue(m_Skybox.WorldMatrix);
#if WINDOWS
            m_EPFogEnabled.SetValue(m_Skybox.FogSupported);
            m_EPFogColor.SetValue(Scene.current.RenderSettings.fogColor);
            m_EPFogData.SetValue(m_Skybox.OverrideFog ? m_Skybox.CustomFogData : Scene.current.RenderSettings.fogData);
#endif
            _effect.CurrentTechnique.Passes[0].Apply();
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
        }
    }
}
