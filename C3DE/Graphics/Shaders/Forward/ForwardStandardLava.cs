using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandardLava : ForwardStandardBase
    {
        private StandardLavaMaterial _material;
        private Vector2 _features;

        public bool EmissiveEnabled => false;

        public ForwardStandardLava(StandardLavaMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/StandardLava");
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            BindCommonMaterialParameters(ref worldMatrix, receiveShadow, drawInstanced);
            BindStandardSurfaceParameters(
                _material.MainTexture,
                Vector3.One,
                _material.SpecularColor.ToVector3(),
                _material.SpecularPower,
                _material.SpecularIntensity,
                _material.Tiling);
            _effect.Parameters["TotalTime"].SetValue(Time.TotalTime * _material.Speed);

            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.SpecularMap != null ? 1 : 0;
            BindStandardOptionalMaps(new Vector4(_features.X, _features.Y, 0.0f, 0.0f), _material.NormalMap, _material.SpecularMap);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
