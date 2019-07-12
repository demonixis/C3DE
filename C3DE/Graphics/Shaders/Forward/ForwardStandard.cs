using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandard : ForwardStandardBase
    {
        private StandardMaterial _Material;
        protected EffectParameter _EPNormalTexture;
        protected EffectParameter _EPNormalTextureEnabled;
        protected EffectParameter _EPReflectionTexture;
        protected EffectParameter _EPReflectionTextureEnabled;
        protected EffectParameter _EPEmissiveTextureEnabled;
        protected EffectParameter _EPEmissiveTexture;
        protected EffectParameter _EPEmissiveColor;
        protected EffectParameter _EPEmissiveIntensity;

        public bool EmissiveEnabled => _Material.EmissiveEnabled;

        public ForwardStandard(StandardMaterial material)
        {
            _Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Standard");
            SetupParamaters();
        }

        protected override void SetupParamaters()
        {
            base.SetupParamaters();

            _EPNormalTexture = _effect.Parameters["NormalTexture"];
            _EPNormalTextureEnabled = _effect.Parameters["NormalTextureEnabled"];
            _EPReflectionTexture = _effect.Parameters["ReflectionTexture"];
            _EPReflectionTextureEnabled = _effect.Parameters["ReflectionTextureEnabled"];
            _EPEmissiveTextureEnabled = _effect.Parameters["EmissiveTextureEnabled"];
            _EPEmissiveTexture = _effect.Parameters["EmissiveTexture"];
            _EPEmissiveColor = _effect.Parameters["EmissiveColor"];
            _EPEmissiveIntensity = _effect.Parameters["EmissiveIntensity"];
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _EPReflectionTexture.SetValue(_Material.ReflectionTexture);
            _EPReflectionTextureEnabled.SetValue(_Material.ReflectionTexture != null);

            _EPEmissiveTextureEnabled.SetValue(_Material.EmissiveTexture != null);
            _EPEmissiveTexture.SetValue(_Material.EmissiveTexture);
            _EPEmissiveColor.SetValue(_Material.EmissiveColor.ToVector3());
            _EPEmissiveIntensity.SetValue(_Material.EmissiveIntensity);
            _EPNormalTexture.SetValue(_Material.NormalMap);
            _EPNormalTextureEnabled.SetValue(_Material.NormalMap != null);

            BasePass(_Material, ref worldMatrix, receiveShadow);
        }
    }
}
