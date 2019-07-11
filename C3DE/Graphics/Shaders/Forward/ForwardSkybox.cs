using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
   public class ForwardSkybox : ShaderMaterial
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

        public ForwardSkybox(Skybox skybox)
        {
            m_Skybox = skybox;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Skybox");
            m_DefaultPass = _effect.CurrentTechnique.Passes["AmbientPass"];
            m_EPView = _effect.Parameters["View"];
            m_EPProjection = _effect.Parameters["Projection"];
            m_EPMainTexture = _effect.Parameters["MainTexture"];
            m_EPEyePosition = _effect.Parameters["EyePosition"];
            m_EPWorld = _effect.Parameters["World"];
            m_EPFogEnabled = _effect.Parameters["FogEnabled"];
            m_EPFogColor = _effect.Parameters["FogColor"];
            m_EPFogData = _effect.Parameters["FogData"];
        }

        public override void Pass(Renderer renderable)
        {
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera._viewMatrix);
            m_EPProjection.SetValue(camera._projectionMatrix);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
            m_EPMainTexture.SetValue(m_Skybox.Texture);
            m_EPWorld.SetValue(m_Skybox.WorldMatrix);
#if !DESKTOP && !ANDROID
            m_EPFogEnabled.SetValue(m_Skybox.FogSupported);
            m_EPFogColor.SetValue(Scene.current.RenderSettings.fogColor);
            m_EPFogData.SetValue(m_Skybox.OverrideFog ? m_Skybox.CustomFogData : Scene.current.RenderSettings.fogData);
#endif
            m_DefaultPass.Apply();
        }
    }
}
