using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardPBRTerrain : ForwardPBRBase
    {
        private PBRTerrainMaterial _material;
        private Vector2 _features;

        public ForwardPBRTerrain(PBRTerrainMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/PBRTerrain");
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _features.X = 
                _material.GrassNormalMap != null &&
                _material.RockNormalMap != null &&
                _material.SandNormalMap != null &&
                _material.SnowNormalMap != null ? 1 : 0;
            _features.Y = 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["GrassMap"].SetValue(_material.GrassMap);
            _effect.Parameters["GrassNormalMap"].SetValue(_material.GrassNormalMap);
            _effect.Parameters["SandMap"].SetValue(_material.SandMap);
            _effect.Parameters["SandNormalMap"].SetValue(_material.SandNormalMap);
            _effect.Parameters["RockMap"].SetValue(_material.RockMap);
            _effect.Parameters["RockNormalMap"].SetValue(_material.RockNormalMap);
            _effect.Parameters["SnowMap"].SetValue(_material.SnowMap);
            _effect.Parameters["SnowNormalMap"].SetValue(_material.SnowNormalMap);
            _effect.Parameters["Roughness"].SetValue(_material.Roughness);
            _effect.Parameters["Metallic"].SetValue(_material.Metallic);
            _effect.Parameters["WeightMap"].SetValue(_material.WeightMap);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(receiveShadow);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
