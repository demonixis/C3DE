using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using C3DE.Components.Lighting;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class TerrainMaterial : Material, IMultipassLightingMaterial
    {
        private Vector3 m_SpecularColor = new Vector3(0.6f, 0.6f, 0.6f);

        private EffectPass m_PassAmbient;
        private EffectPass m_PassLight;
        private EffectParameter m_EPView;
        private EffectParameter m_EPProjection;
        private EffectParameter m_EPEyePosition;
        private EffectParameter m_EPAmbientColor;
        private EffectParameter m_EPWorld;
        private EffectParameter m_EPTextureTilling;
        private EffectParameter m_EPDiffuseColor;
        private EffectParameter m_EPMainTexture;
        private EffectParameter m_EPWeightTexture;
        private EffectParameter m_EPRockTexture;
        private EffectParameter m_EPSnowTexture;
        private EffectParameter m_EPSandTexture;
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

        public Texture2D SnowTexture { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D WeightTexture { get; set; }

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

        public TerrainMaterial(Scene scene, string name = "Terrain Material")
            : base(scene)
        {
            base.m_DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            Tiling = Vector2.One;
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/TerrainEffect");

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
            m_EPWeightTexture = m_Effect.Parameters["WeightMap"];
            m_EPRockTexture = m_Effect.Parameters["RockTexture"];
            m_EPSnowTexture = m_Effect.Parameters["SnowTexture"];
            m_EPSandTexture = m_Effect.Parameters["SandTexture"];
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
            m_EPWeightTexture.SetValue(WeightTexture);
            m_EPSnowTexture.SetValue(SnowTexture);
            m_EPSandTexture.SetValue(SandTexture);
            m_EPRockTexture.SetValue(RockTexture);

            m_PassAmbient.Apply();
        }

        public void LightPass(Renderer renderer, Light light)
        {
            m_EPSpecularLightColor.SetValue(m_SpecularColor);
            m_EPSpecularPower.SetValue(Shininess);
            m_EPSpecularIntensity.SetValue(SpecularIntensity);
            m_EPLightColor.SetValue(light.color);
            m_EPLightDirection.SetValue(light.transform.Forward);
            m_EPLightPosition.SetValue(light.transform.LocalPosition);
            m_EPLightSpotAngle.SetValue(light.Angle);
            m_EPLightIntensity.SetValue(light.Intensity);
            m_EPLightRange.SetValue(light.Range);
            m_EPLightFallOff.SetValue(light.FallOf);
            m_EPLightType.SetValue((int)light.TypeLight);
            m_EPShadowStrength.SetValue(light.shadowGenerator.ShadowStrength);
            m_EPShadowBias.SetValue(light.shadowGenerator.ShadowBias);
            m_EPShadowMap.SetValue(light.shadowGenerator.ShadowMap);
            m_EPShadowEnabled.SetValue(renderer.ReceiveShadow);
            m_EPFogColor.SetValue(scene.RenderSettings.fogColor);
            m_EPFogData.SetValue(scene.RenderSettings.fogData);
            m_EPLightView.SetValue(light.viewMatrix);
            m_EPLightProjection.SetValue(light.projectionMatrix);

            m_PassLight.Apply();
        }
    }
}
