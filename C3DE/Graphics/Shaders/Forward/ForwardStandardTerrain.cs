using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandardTerrain : ForwardStandardBase
    {
        private StandardTerrainMaterial _material;
        private Vector2 _features;

        public ForwardStandardTerrain(StandardTerrainMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/StandardTerrain");
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            var hasNormalMap = 
                _material.GrassNormalMap != null &&
                _material.RockNormalMap != null &&
                _material.SandNormalMap != null &&
                _material.SnownNormalMap != null;

            _features.X = hasNormalMap ? 1 : 0;
            _features.Y = 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["WeightMap"].SetValue(_material.WeightTexture);
            _effect.Parameters["GrassMap"].SetValue(_material.MainTexture);
            _effect.Parameters["SandMap"].SetValue(_material.SandTexture);
            _effect.Parameters["RockMap"].SetValue(_material.RockTexture);
            _effect.Parameters["SnowMap"].SetValue(_material.SnowTexture);
            _effect.Parameters["SpecularColor"].SetValue(_material._specularColor.X);
            _effect.Parameters["SpecularPower"].SetValue(_material.SpecularPower);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(receiveShadow);

            if (hasNormalMap)
            {
                _effect.Parameters["GrassNormalMap"].SetValue(_material.GrassNormalMap);
                _effect.Parameters["SandNormalMap"].SetValue(_material.SandNormalMap);
                _effect.Parameters["RockNormalMap"].SetValue(_material.RockNormalMap);
                _effect.Parameters["SnowNormalMap"].SetValue(_material.SnownNormalMap);
            }

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
