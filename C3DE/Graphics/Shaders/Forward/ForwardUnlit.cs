using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardUnlit : ForwardShader
    {
        private UnlitMaterial m_Material;
        private EffectPass _passColor;
        private EffectPass _passTexture;
        private EffectParameter _EPView;
        private EffectParameter _EPProjection;
        private EffectParameter _EPWorld;
        private EffectParameter _EPTextureTilling;
        private EffectParameter _EPDiffuseColor;
        private EffectParameter _EPMainTexture;

        public ForwardUnlit(UnlitMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Unlit");
            SetupParameters();
        }

        protected void SetupParameters()
        {
            _passColor = _effect.CurrentTechnique.Passes["UnlitColor"];
            _passTexture = _effect.CurrentTechnique.Passes["UnlitTexture"];
            _EPView = _effect.Parameters["View"];
            _EPProjection = _effect.Parameters["Projection"];
            _EPWorld = _effect.Parameters["World"];
            _EPTextureTilling = _effect.Parameters["TextureTilling"];
            _EPDiffuseColor = _effect.Parameters["DiffuseColor"];
            _EPMainTexture = _effect.Parameters["MainTexture"];
        }

        public override void PrePass(ref Vector3 cameraPosition, ref Matrix viewMatrix, ref Matrix projectionMatrix, ref LightData lightData, ref ShadowData shadowData)
        {
            _EPView.SetValue(viewMatrix);
            _EPProjection.SetValue(projectionMatrix);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _EPWorld.SetValue(worldMatrix);
            _EPTextureTilling.SetValue(m_Material.Tiling);
            _EPDiffuseColor.SetValue(m_Material._diffuseColor);
            _EPMainTexture.SetValue(m_Material.MainTexture);

            if (m_Material.MainTexture == null)
                _passColor.Apply();
            else
                _passTexture.Apply();
        }
    }
}
