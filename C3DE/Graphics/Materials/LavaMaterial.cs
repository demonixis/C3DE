using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class LavaMaterial : Material, IEmissiveMaterial
    {
        private EffectPass m_PassAmbient;
        private EffectPass m_PassEmissive;
        private EffectParameter m_EPView;
        private EffectParameter m_EPProjection;
        private EffectParameter m_EPTime;
        private EffectParameter m_EPMainTexture;
        private EffectParameter m_EPNormalTexture;
        private EffectParameter m_EPTextureTilling;
        private EffectParameter m_EPDiffuseColor;
        private EffectParameter m_EPWorld;
        private EffectParameter m_EPEmissiveIntensity;

        public Texture2D NormalTexture { get; set; }

        public float EmissiveIntensity { get; set; } = 2.0f;

        public bool EmissiveEnabled => false;

        public float Speed { get; set; } = 0.25f;

        public LavaMaterial(Scene scene, string name = "Lava Material")
            : base(scene)
        {
            Name = name;
        }

        public override void LoadContent(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/LavaEffect");
            m_PassAmbient = m_Effect.CurrentTechnique.Passes["AmbientPass"];
            m_PassEmissive = m_Effect.CurrentTechnique.Passes["EmissivePass"];

            m_EPView = m_Effect.Parameters["View"];
            m_EPProjection = m_Effect.Parameters["Projection"];
            m_EPTime = m_Effect.Parameters["Time"];
            m_EPMainTexture = m_Effect.Parameters["MainTexture"];
            m_EPNormalTexture = m_Effect.Parameters["NormalTexture"];
            m_EPTextureTilling = m_Effect.Parameters["TextureTiling"];
            m_EPDiffuseColor = m_Effect.Parameters["DiffuseColor"];
            m_EPWorld = m_Effect.Parameters["World"];
            m_EPEmissiveIntensity = m_Effect.Parameters["EmissiveIntensity"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera.view);
            m_EPProjection.SetValue(camera.projection);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPTime.SetValue(Time.TotalTime * Speed);
            m_EPMainTexture.SetValue(MainTexture);
            m_EPNormalTexture.SetValue(NormalTexture);
            m_EPTextureTilling.SetValue(Tiling);
            m_EPDiffuseColor.SetValue(m_DiffuseColor);
            m_EPWorld.SetValue(renderable.Transform.m_WorldMatrix);
            m_EPEmissiveIntensity.SetValue(EmissiveIntensity);
            m_PassAmbient.Apply();
        }

        public void EmissivePass(Renderer renderer)
        {
        }
    }
}
