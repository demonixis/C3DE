using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class LPPStandard : LPPShader
    {
        private StandardMaterial m_Material;
        protected EffectPass m_PassLight;
        protected EffectParameter m_EPView;
        protected EffectParameter m_EPProjection;
        protected EffectParameter m_EPEyePosition;
        protected EffectParameter m_EPAmbientColor;
        protected EffectParameter m_EPWorld;
        protected EffectParameter m_EPTextureTilling;
        protected EffectParameter m_EPDiffuseColor;
        protected EffectParameter m_EPMainTexture;
        protected EffectParameter m_EPReflectionTexture;
        protected EffectParameter m_EPReflectionTextureEnabled;
        protected EffectParameter m_EPLightMap;
        protected EffectParameter m_EPViewport;

        public LPPStandard(StandardMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/LPP/Standard");
            m_PassLight = m_Effect.CurrentTechnique.Passes[0];
            m_EPView = m_Effect.Parameters["View"];
            m_EPProjection = m_Effect.Parameters["Projection"];
            m_EPEyePosition = m_Effect.Parameters["EyePosition"];
            m_EPAmbientColor = m_Effect.Parameters["AmbientColor"];
            m_EPWorld = m_Effect.Parameters["World"];
            m_EPTextureTilling = m_Effect.Parameters["TextureTiling"];
            m_EPDiffuseColor = m_Effect.Parameters["DiffuseColor"];
            m_EPMainTexture = m_Effect.Parameters["MainTexture"];
            m_EPReflectionTexture = m_Effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = m_Effect.Parameters["ReflectionTextureEnabled"];
            m_EPLightMap = m_Effect.Parameters["LightMap"];
            m_EPViewport = m_Effect.Parameters["Viewport"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera.m_ViewMatrix);
            m_EPProjection.SetValue(camera.m_ProjectionMatrix);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
            m_EPAmbientColor.SetValue(Scene.current.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
        }

        public override void Pass(Renderer renderable, RenderTarget2D lightmap)
        {
            m_EPLightMap.SetValue(lightmap);
            m_EPViewport.SetValue(new Vector2(Screen.Width, Screen.Height));
            m_EPReflectionTexture.SetValue(m_Material.ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(m_Material.ReflectionTexture != null);
            m_EPWorld.SetValue(renderable.Transform.m_WorldMatrix);
            m_EPTextureTilling.SetValue(m_Material.Tiling);
            m_EPDiffuseColor.SetValue(m_Material.m_DiffuseColor);
            m_EPMainTexture.SetValue(m_Material.MainTexture);
            m_PassLight.Apply();
        }
    }
}
