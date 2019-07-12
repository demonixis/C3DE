using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class PBRTerrainMaterial : Material
    {
        public Texture2D GrassMap { get; set; }
        public Texture2D SandMap { get; set; }
        public Texture2D RockMap { get; set; }
        public Texture2D SnowMap { get; set; }
        public Texture2D GrassNormalMap { get; set; }
        public Texture2D SandNormalMap { get; set; }
        public Texture2D RockNormalMap { get; set; }
        public Texture2D SnowNormalMap { get; set; }
        public Texture2D WeightMap { get; set; }

        public float Roughness { get; set; } = 0.5f;
        public float Metallic { get; set; } = 0.0f;

        public PBRTerrainMaterial() : base() { }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            _shaderMaterial = new ForwardPBRTerrain(this);
            _shaderMaterial.LoadEffect(Application.Content);
        }

        private static Color GetColor(float value) => new Color(value, value, value, 1);
    }
}
