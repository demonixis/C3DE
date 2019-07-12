using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardLava : ForwardShader
    {
        private LavaMaterial m_Material;
        private EffectParameter _EPView;
        private EffectParameter _EPProjection;
        private EffectParameter _EPTime;
        private EffectParameter _EPMainTexture;
        private EffectParameter _EPNormalTexture;
        private EffectParameter _EPTextureTilling;
        private EffectParameter _EPDiffuseColor;
        private EffectParameter _EPWorld;
        private EffectParameter _EPEmissiveIntensity;

        public bool EmissiveEnabled => false;

        public ForwardLava(LavaMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Lava");
    
        }

        protected void SetupParameters()
        {
            _EPView = _effect.Parameters["View"];
            _EPProjection = _effect.Parameters["Projection"];
            _EPTime = _effect.Parameters["Time"];
            _EPMainTexture = _effect.Parameters["MainTexture"];
            _EPNormalTexture = _effect.Parameters["NormalTexture"];
            _EPTextureTilling = _effect.Parameters["TextureTiling"];
            _EPDiffuseColor = _effect.Parameters["DiffuseColor"];
            _EPWorld = _effect.Parameters["World"];
            _EPEmissiveIntensity = _effect.Parameters["EmissiveIntensity"];
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData)
        {
            _EPView.SetValue(viewMatrix);
            _EPProjection.SetValue(projectionMatrix);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _EPTime.SetValue(Time.TotalTime * m_Material.Speed);
            _EPMainTexture.SetValue(m_Material.MainTexture);
            _EPNormalTexture.SetValue(m_Material.NormalMap);
            _EPTextureTilling.SetValue(m_Material.Tiling);
            _EPDiffuseColor.SetValue(m_Material._diffuseColor);
            _EPWorld.SetValue(worldMatrix);
            _EPEmissiveIntensity.SetValue(m_Material.EmissiveIntensity);
            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
