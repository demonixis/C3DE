using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class StandardTerrainMaterial : StandardMaterialBase
    {
        public Texture2D SnowTexture { get; set; }
        public Texture2D SnownNormalMap { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D SandNormalMap { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D RockNormalMap { get; set; }
        public Texture2D WeightTexture { get; set; }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                _shaderMaterial = new ForwardStandardTerrain(this);
            else if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredStandardTerrain(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
