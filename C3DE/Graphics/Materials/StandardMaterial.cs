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
    public class StandardMaterial : Material, IMultipassLightingMaterial, IEmissiveMaterial
    {
        private Vector3 m_EmissiveColor = Vector3.Zero;
        private Vector3 m_SpecularColor = new Vector3(0.6f, 0.6f, 0.6f);

        private EffectPass m_PassAmbient;
        private EffectPass m_PassLight;
        private EffectPass m_PassEmissive;
        private EffectParameter m_EPView;
        private EffectParameter m_EPProjection;
        private EffectParameter m_EPEyePosition;
        private EffectParameter m_EPAmbientColor;
        private EffectParameter m_EPWorld;
        private EffectParameter m_EPTextureTilling;
        private EffectParameter m_EPDiffuseColor;
        private EffectParameter m_EPMainTexture;
        private EffectParameter m_EPReflectionTexture;
        private EffectParameter m_EPReflectionTextureEnabled;
        private EffectParameter m_EPReflectionIntensity;
        private EffectParameter m_EPSpecularLightColor;
        private EffectParameter m_EPSpecularPower;
        private EffectParameter m_EPSpecularIntensity;
        private EffectParameter m_EPLightColor;
        private EffectParameter m_EPLightDirection;
        private EffectParameter m_EPLightPosition;
        private EffectParameter m_EPLightSpotAngle;
        private EffectParameter m_EPLightIntensity;
        private EffectParameter m_EPLightRange;
        private EffectParameter m_EPLightFallOff;
        private EffectParameter m_EPLightType;
        private EffectParameter m_EPShadowStrength;
        private EffectParameter m_EPShadowBias;
        private EffectParameter m_EPShadowMap;
        private EffectParameter m_EPShadowEnabled;
        private EffectParameter m_EPFogColor;
        private EffectParameter m_EPFogData;
        private EffectParameter m_EPLightView;
        private EffectParameter m_EPLightProjection;
        private EffectParameter m_EPSpecularTextureEnabled;
        private EffectParameter m_EPSpecularTexture;
        private EffectParameter m_EPEmissiveTextureEnabled;
        private EffectParameter m_EPEmissiveTexture;
        private EffectParameter m_EPEmissiveColor;
        private EffectParameter m_EPEmissiveIntensity;

        public Texture2D EmissiveTexture { get; set; }

        public TextureCube ReflectionTexture { get; set; }

        [DataMember]
        public float ReflectionIntensity { get; set; } = 0.35f;

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

        [DataMember]
        public Color EmissiveColor
        {
            get { return new Color(m_EmissiveColor); }
            set { m_EmissiveColor = value.ToVector3(); }
        }

        [DataMember]
        public bool EmissiveEnabled { get; set; } = false;

        [DataMember]
        public float EmissiveIntensity { get; set; } = 1.0f;

        public StandardMaterial(Scene scene, string name = "Standard Material")
            : base(scene)
        {
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/StandardEffect");

            m_PassAmbient = m_Effect.CurrentTechnique.Passes["AmbientPass"];
            m_PassLight = m_Effect.CurrentTechnique.Passes["LightPass"];
            m_PassEmissive = m_Effect.CurrentTechnique.Passes["EmissivePass"];

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
            m_EPReflectionIntensity = m_Effect.Parameters["ReflectionIntensity"];
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
            m_EPEmissiveTextureEnabled = m_Effect.Parameters["EmissiveTextureEnabled"];
            m_EPEmissiveTexture = m_Effect.Parameters["EmissiveTexture"];
            m_EPEmissiveColor = m_Effect.Parameters["EmissiveColor"];
            m_EPEmissiveIntensity = m_Effect.Parameters["EmissiveIntensity"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera.view);
            m_EPProjection.SetValue(camera.projection);
            m_EPEyePosition.SetValue(camera.Transform.LocalPosition);
            m_EPAmbientColor.SetValue(scene.RenderSettings.ambientColor);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPWorld.SetValue(renderable.Transform.world);
            m_EPTextureTilling.SetValue(Tiling);
            m_EPDiffuseColor.SetValue(m_DiffuseColor);
            m_EPMainTexture.SetValue(MainTexture);

            // High
            m_EPReflectionTexture.SetValue(ReflectionTexture);
            m_EPReflectionTextureEnabled.SetValue(ReflectionTexture != null);
            m_EPReflectionIntensity.SetValue(ReflectionIntensity);

            m_PassAmbient.Apply();
        }

        public void LightPass(Renderer renderer, Light light)
        {
            m_EPSpecularLightColor.SetValue(m_SpecularColor);
            m_EPSpecularPower.SetValue(Shininess);
            m_EPSpecularIntensity.SetValue(SpecularIntensity);
            m_EPLightColor.SetValue(light.color);
            m_EPLightDirection.SetValue(light.transform.LocalRotation);
            m_EPLightPosition.SetValue(light.transform.LocalPosition);
            m_EPLightSpotAngle.SetValue(light.Angle);
            m_EPLightIntensity.SetValue(light.Intensity);
            m_EPLightRange.SetValue(light.Range);
            m_EPLightFallOff.SetValue(light.FallOf);
            m_EPLightType.SetValue((int)light.TypeLight);

            // High
            m_EPShadowStrength.SetValue(light.shadowGenerator.ShadowStrength);
            m_EPShadowBias.SetValue(light.shadowGenerator.ShadowBias);
            m_EPShadowMap.SetValue(light.shadowGenerator.ShadowMap);
            m_EPShadowEnabled.SetValue(renderer.ReceiveShadow);
            m_EPFogColor.SetValue(scene.RenderSettings.fogColor);
            m_EPFogData.SetValue(scene.RenderSettings.fogData);
            m_EPLightView.SetValue(light.viewMatrix);
            m_EPLightProjection.SetValue(light.projectionMatrix);
            m_EPSpecularTextureEnabled.SetValue(SpecularTexture != null);
            m_EPSpecularTexture.SetValue(SpecularTexture);

            m_PassLight.Apply();
        }

        public void EmissivePass(Renderer renderer)
        {
            m_EPEmissiveTextureEnabled.SetValue(EmissiveTexture != null);
            m_EPEmissiveTexture.SetValue(EmissiveTexture);
            m_EPEmissiveColor.SetValue(m_EmissiveColor);
            m_EPEmissiveIntensity.SetValue(EmissiveIntensity);
            m_PassEmissive.Apply();
        }
    }
}
