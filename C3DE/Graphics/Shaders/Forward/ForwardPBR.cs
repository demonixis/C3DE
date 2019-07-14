using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardPBR : ForwardPBRBase
    {
        private PBRMaterial _material;
        private Vector3 _features;

        public ForwardPBR(PBRMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/PBRStandard");
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.EmissiveMap != null ? 1 : 0;
            _features.Z = _material.CutoutEnabled ? 1 : 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
            _effect.Parameters["RMAOMap"].SetValue(_material._rmaoMap);
            _effect.Parameters["EmissiveMap"].SetValue(_material.EmissiveMap);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(receiveShadow);
            _effect.Parameters["DiffuseColor"].SetValue(_material._diffuseColor);
            _effect.Parameters["Cutout"].SetValue(_material.Cutout);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
