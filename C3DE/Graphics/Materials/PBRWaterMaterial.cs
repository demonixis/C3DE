using C3DE.Graphics.Materials.Shaders.Forward;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static C3DE.Graphics.TextureFactory;

namespace C3DE.Graphics.Materials
{
    public class PBRWaterMaterial : Material
    {
        internal Texture2D _rmaoMap;

        public float Alpha { get; set; } = 0.65f;
        public Texture2D NormalMap { get; set; }

        public Texture2D RoughnessMetalicAOMap
        {
            get => _rmaoMap;
            set => _rmaoMap = value;
        }

        public PBRWaterMaterial() : base() { }

        public void CreateRoughnessMetallicAO(float roughness = 0.5f, float metallic = 0.5f, float ao = 1.0f)
        {
            if (_rmaoMap != null)
                _rmaoMap.Dispose();

            _rmaoMap = TextureFactory.CreateRoughnessMetallicAO(
                CreateColor(GetColor(roughness), 1, 1),
                CreateColor(GetColor(metallic), 1, 1),
                CreateColor(GetColor(ao), 1, 1));
        }

        public void CreateRoughnessMetallicAO(Texture2D roughness, Texture2D metallic, Texture2D ao)
        {
            if (_rmaoMap != null)
                _rmaoMap.Dispose();

            _rmaoMap = TextureFactory.CreateRoughnessMetallicAO(roughness, metallic, ao);
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            _shaderMaterial = new ForwardPBRWater(this);
            _shaderMaterial.LoadEffect(Application.Content);
        }

        private static Color GetColor(float value) => new Color(value, value, value, 1);
    }
}
