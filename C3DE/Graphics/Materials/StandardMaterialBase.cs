using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class StandardMaterialBase : Material, IMultipassLightingMaterial
    {
        protected Vector3 m_SpecularColor = new Vector3(0.6f, 0.6f, 0.6f);
        protected EffectPass m_PassAmbient;
        protected EffectPass m_PassLight;
        protected EffectParameter m_EPView;
        protected EffectParameter m_EPProjection;
        protected EffectParameter m_EPEyePosition;
        protected EffectParameter m_EPAmbientColor;
        protected EffectParameter m_EPWorld;
        protected EffectParameter m_EPTextureTilling;
        protected EffectParameter m_EPDiffuseColor;
        protected EffectParameter m_EPMainTexture;
        protected EffectParameter m_EPSpecularLightColor;
        protected EffectParameter m_EPSpecularPower;
        protected EffectParameter m_EPSpecularIntensity;
        protected EffectParameter m_EPLightColor;
        protected EffectParameter m_EPLightDirection;
        protected EffectParameter m_EPLightPosition;
        protected EffectParameter m_EPLightSpotAngle;
        protected EffectParameter m_EPLightIntensity;
        protected EffectParameter m_EPLightRange;
        protected EffectParameter m_EPLightFallOff;
        protected EffectParameter m_EPLightType;
        protected EffectParameter m_EPShadowStrength;
        protected EffectParameter m_EPShadowBias;
        protected EffectParameter m_EPShadowMap;
        protected EffectParameter m_EPShadowEnabled;
        protected EffectParameter m_EPFogColor;
        protected EffectParameter m_EPFogData;
        protected EffectParameter m_EPLightView;
        protected EffectParameter m_EPLightProjection;
        protected EffectParameter m_EPSpecularTextureEnabled;
        protected EffectParameter m_EPSpecularTexture;

        public Texture2D NormalTexture { get; set; }

        public Texture2D SpecularTexture { get; set; }

        [DataMember]
        public Color SpecularColor
        {
            get { return new Color(m_SpecularColor); }
            set { m_SpecularColor = value.ToVector3(); }
        }

        [DataMember]
        public float SpecularIntensity { get; set; } = 1.0f;

        [DataMember]
        public float Shininess { get; set; } = 250.0f;

        public StandardMaterialBase(Scene scene, string name = "Standard Material Base")
            : base(scene)
        {
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            LoadEffect(content);
            SetupParamaters();
        }

        protected virtual void LoadEffect(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/StandardEffect");
        }

        protected virtual void SetupParamaters()
        {
            m_PassAmbient = m_Effect.CurrentTechnique.Passes["AmbientPass"];
            m_PassLight = m_Effect.CurrentTechnique.Passes["LightPass"];
            m_EPView = m_Effect.Parameters["View"];
            m_EPProjection = m_Effect.Parameters["Projection"];
            m_EPEyePosition = m_Effect.Parameters["EyePosition"];
            m_EPAmbientColor = m_Effect.Parameters["AmbientColor"];
            m_EPWorld = m_Effect.Parameters["World"];
            m_EPTextureTilling = m_Effect.Parameters["TextureTiling"];
            m_EPDiffuseColor = m_Effect.Parameters["DiffuseColor"];
            m_EPMainTexture = m_Effect.Parameters["MainTexture"];
            m_EPSpecularLightColor = m_Effect.Parameters["SpecularLightColor"];
            m_EPSpecularPower = m_Effect.Parameters["SpecularPower"];
            m_EPSpecularIntensity = m_Effect.Parameters["SpecularIntensity"];
            m_EPLightColor = m_Effect.Parameters["LightColor"];
            m_EPLightDirection = m_Effect.Parameters["LightDirection"];
            m_EPLightPosition = m_Effect.Parameters["LightPosition"];
            m_EPLightSpotAngle = m_Effect.Parameters["LightSpotAngle"];
            m_EPLightIntensity = m_Effect.Parameters["LightIntensity"];
            m_EPLightRange = m_Effect.Parameters["LightRange"];
            m_EPLightFallOff = m_Effect.Parameters["LightFallOff"];
            m_EPLightType = m_Effect.Parameters["LightType"];
            m_EPShadowStrength = m_Effect.Parameters["ShadowStrength"];
            m_EPShadowBias = m_Effect.Parameters["ShadowBias"];
            m_EPShadowMap = m_Effect.Parameters["ShadowMap"];
            m_EPShadowEnabled = m_Effect.Parameters["ShadowEnabled"];
            m_EPFogColor = m_Effect.Parameters["FogColor"];
            m_EPFogData = m_Effect.Parameters["FogData"];
            m_EPLightView = m_Effect.Parameters["LightView"];
            m_EPLightProjection = m_Effect.Parameters["LightProjection"];
            m_EPSpecularTextureEnabled = m_Effect.Parameters["SpecularTextureEnabled"];
            m_EPSpecularTexture = m_Effect.Parameters["SpecularTexture"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera.m_ViewMatrix);
            m_EPProjection.SetValue(camera.m_ProjectionMatrix);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
            m_EPAmbientColor.SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPWorld.SetValue(renderable.Transform.m_WorldMatrix);
            m_EPTextureTilling.SetValue(Tiling);
            m_EPDiffuseColor.SetValue(m_DiffuseColor);
            m_EPMainTexture.SetValue(MainTexture);
            m_PassAmbient.Apply();
        }

        public virtual void LightPass(Renderer renderer, Light light)
        {
            m_EPSpecularLightColor.SetValue(m_SpecularColor);
            m_EPSpecularPower.SetValue(Shininess);
            m_EPSpecularIntensity.SetValue(SpecularIntensity);
            m_EPLightColor.SetValue(light.m_Color);
            m_EPLightDirection.SetValue(light.m_Transform.LocalRotation);
            m_EPLightPosition.SetValue(light.m_Transform.LocalPosition);
            m_EPLightSpotAngle.SetValue(light.Angle);
            m_EPLightIntensity.SetValue(light.Intensity);
            m_EPLightRange.SetValue(light.Range);
            m_EPLightFallOff.SetValue(light.FallOf);
            m_EPLightType.SetValue((int)light.TypeLight);

            m_EPShadowStrength.SetValue(light.m_ShadowGenerator.ShadowStrength);
            m_EPShadowBias.SetValue(light.m_ShadowGenerator.ShadowBias);
            m_EPShadowMap.SetValue(light.m_ShadowGenerator.ShadowMap);
            m_EPShadowEnabled.SetValue(renderer.ReceiveShadow);
            m_EPFogColor.SetValue(scene.RenderSettings.fogColor);
            m_EPFogData.SetValue(scene.RenderSettings.fogData);
            m_EPLightView.SetValue(light.m_ViewMatrix);
            m_EPLightProjection.SetValue(light.m_ProjectionMatrix);
            m_EPSpecularTextureEnabled.SetValue(SpecularTexture != null);
            m_EPSpecularTexture.SetValue(SpecularTexture);

            m_PassLight.Apply();
        }
    }
}
