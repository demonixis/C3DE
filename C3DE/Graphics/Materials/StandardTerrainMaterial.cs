using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class StandardTerrainMaterial : Material
    {
        internal Vector3 _specularColor = new Vector3(0.6f, 0.6f, 0.6f);

        public Texture2D GrassNormalMap { get; set; }
        public Texture2D SnowTexture { get; set; }
        public Texture2D SnownNormalMap { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D SandNormalMap { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D RockNormalMap { get; set; }
        public Texture2D WeightTexture { get; set; }

        public float SpecularPower { get; set; } = 250.0f;

        public Color SpecularColor
        {
            get => new Color(_specularColor);
            set => _specularColor = value.ToVector3();
        }

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
