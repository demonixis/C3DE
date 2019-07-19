using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardPBRAtlasedTerrain : ForwardPBRBase
    {
        private PBRTerrainAtlasedMaterial _material;
        private Vector2 _features;

        public ForwardPBRAtlasedTerrain(PBRTerrainAtlasedMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/PBRTerrainAtlased");
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _features.X = _material._combinedNormals != null ? 1 : 0;
            _features.Y = 0;

            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["CombinedAlbedos"].SetValue(_material._combinedAlbedos);
            _effect.Parameters["CombinedNormals"].SetValue(_material._combinedNormals);
            _effect.Parameters["CombinedRMAOs"].SetValue(_material._combinedRMAO);
            _effect.Parameters["WeightMap"].SetValue(_material.WeightMap);
            _effect.Parameters["Features"].SetValue(_features);
            _effect.Parameters["ShadowEnabled"].SetValue(receiveShadow);

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
