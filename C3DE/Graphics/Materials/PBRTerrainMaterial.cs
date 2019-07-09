using C3DE.Graphics.Materials.Shaders.Forward;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.Materials
{
    public class PBRTerrainMaterial : Material
    {
        internal Texture2D _rmaoMap;

        public Texture2D GrassNormalMap { get; set; }
        public Texture2D SnowTexture { get; set; }
        public Texture2D SnownNormalMap { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D SandNormalMap { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D RockNormalMap { get; set; }
        public Texture2D WeightTexture { get; set; }

        public Texture2D RoughnessMetalicSpecularAOMap => _rmaoMap;

        public PBRTerrainMaterial() : base() { }
        public PBRTerrainMaterial(string name) : base(name) { }

        public void CreateRoughnessMetallicAO(float roughness = 0.5f, float metallic = 0.5f, float ao = 1.0f)
        {
            _rmaoMap = TextureFactory.CreateRoughnessMetallicAO(
                TextureFactory.CreateColor(GetColor(roughness), 1, 1),
                TextureFactory.CreateColor(GetColor(metallic), 1, 1),
                TextureFactory.CreateColor(GetColor(ao), 1, 1));
        }

        public void CreateRoughnessMetallicAO(Texture2D[] roughness, Texture2D[] metallic, Texture2D[] ao)
        {
            if (roughness.Length != 4 || metallic.Length != 4 || ao.Length != 4)
                throw new Exception("Can't create a packed RMAO texture because the array have to contains 4 textures.");

            var textures = new Texture2D[4];
            for (var i = 0; i < 4; i++)
                textures[i] = TextureFactory.CreateRoughnessMetallicAO(roughness[i], metallic[i], ao[i]);

            _rmaoMap = TextureFactory.PackTextures(textures[0], textures[1], textures[2], textures[3]);

#if DEBUG
            // FIXME
            var stream = System.IO.File.Create("rmaoCombined.png");
            _rmaoMap.SaveAsPng(stream, _rmaoMap.Width, _rmaoMap.Height);
#endif
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            _shaderMaterial = new ForwardTerrainPBR(this);
            _shaderMaterial.LoadEffect(Application.Content);
        }

        private static Color GetColor(float value) => new Color(value, value, value, 1);
    }
}
