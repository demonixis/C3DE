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
        protected EffectParameter m_EPEmissiveTextureEnabled;
        protected EffectParameter m_EPEmissiveTexture;
        protected EffectParameter m_EPEmissiveColor;
        protected EffectParameter m_EPEmissiveIntensity;

        public LPPStandard(StandardMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/LPP/Standard");
            m_PassLight = _effect.CurrentTechnique.Passes[0];
            m_EPView = _effect.Parameters["View"];
            m_EPProjection = _effect.Parameters["Projection"];
            m_EPEyePosition = _effect.Parameters["EyePosition"];
            m_EPAmbientColor = _effect.Parameters["AmbientColor"];
            m_EPWorld = _effect.Parameters["World"];
            m_EPTextureTilling = _effect.Parameters["TextureTiling"];
            m_EPDiffuseColor = _effect.Parameters["DiffuseColor"];
            m_EPMainTexture = _effect.Parameters["MainTexture"];
            m_EPReflectionTexture = _effect.Parameters["ReflectionTexture"];
            m_EPReflectionTextureEnabled = _effect.Parameters["ReflectionTextureEnabled"];
            m_EPLightMap = _effect.Parameters["LightMap"];
            m_EPViewport = _effect.Parameters["Viewport"];
            m_EPEmissiveTextureEnabled = _effect.Parameters["EmissiveTextureEnabled"];
            m_EPEmissiveTexture = _effect.Parameters["EmissiveTexture"];
            m_EPEmissiveColor = _effect.Parameters["EmissiveColor"];
            m_EPEmissiveIntensity = _effect.Parameters["EmissiveIntensity"];
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
            m_EPEmissiveTextureEnabled.SetValue(m_Material.EmissiveTexture != null);
            m_EPEmissiveTexture.SetValue(m_Material.EmissiveTexture);
            m_EPEmissiveColor.SetValue(m_Material.EmissiveColor.ToVector3());
            m_EPEmissiveIntensity.SetValue(m_Material.EmissiveIntensity);
            m_EPLightMap.SetValue(lightmap);
            m_EPViewport.SetValue(new Vector2(Screen.Width, Screen.Height));
            m_EPReflectionTexture.SetValue(m_Material.ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(m_Material.ReflectionTexture != null);
            m_EPWorld.SetValue(renderable.Transform.m_WorldMatrix);
            m_EPTextureTilling.SetValue(m_Material.Tiling);
            m_EPDiffuseColor.SetValue(m_Material._diffuseColor);
            m_EPMainTexture.SetValue(m_Material.MainTexture);
            m_PassLight.Apply();
        }
    }
}
