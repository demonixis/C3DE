using C3DE.Graphics.Materials.Shaders.Forward;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.Materials
{
    public class PBRTerrainMaterial : Material
    {
        internal Texture2D _combinedAlbedos;
        internal Texture2D _combinedNormals;
        internal Texture2D _combinedRMAO;

        public Texture2D CombinedAlbedos => _combinedAlbedos;
        public Texture2D CombinedNormals => _combinedNormals;
        public Texture2D CombinedRMAO => _combinedRMAO;
        public Texture2D WeightMap { get; set; }

        public PBRTerrainMaterial() : base() { }
        public PBRTerrainMaterial(string name) : base(name) { }

        public void CreateAlbedos(Texture2D grass, Texture2D sand, Texture2D rock, Texture2D snown, bool superSample)
        {
            _combinedAlbedos?.Dispose();
            _combinedAlbedos = TextureFactory.PackTextures(grass.Width * (superSample ? 4 : 2), grass.Height * (superSample ? 4 : 2), grass, sand, rock, snown);
        }

        public void CreateNormals(Texture2D grassNormal, Texture2D sandNormal, Texture2D rockNormal, Texture2D snownNormal)
        {
            _combinedNormals?.Dispose();
            _combinedNormals = TextureFactory.PackTextures(0, 0, grassNormal, sandNormal, rockNormal, snownNormal);
        }

        public void CreateRoughnessMetallicAO(float roughness = 0.5f, float metallic = 0.5f, float ao = 1.0f)
        {
            _combinedRMAO?.Dispose();
            _combinedRMAO = TextureFactory.CreateRoughnessMetallicAO(
                TextureFactory.CreateColor(GetColor(roughness), 1, 1),
                TextureFactory.CreateColor(GetColor(metallic), 1, 1),
                TextureFactory.CreateColor(GetColor(ao), 1, 1));
        }

        public void CreateRoughnessMetallicAO(Texture2D combinedGrassRMAO, Texture2D combinedSandRMAO, Texture2D combinedRockRMAO, Texture2D combinedSnowRMAO)
        {
            _combinedRMAO?.Dispose();
            _combinedRMAO = TextureFactory.PackTextures(0, 0, combinedGrassRMAO, combinedSandRMAO, combinedRockRMAO, combinedSnowRMAO);
        }

        public void CreateRoughnessMetallicAO(Texture2D[] roughness, Texture2D[] metallic, Texture2D[] ao)
        {
            if (roughness.Length != 4 || metallic.Length != 4 || ao.Length != 4)
                throw new Exception("Can't create a packed RMAO texture because the array have to contains 4 textures.");

            var textures = new Texture2D[4];
            for (var i = 0; i < 4; i++)
                textures[i] = TextureFactory.CreateRoughnessMetallicAO(roughness[i], metallic[i], ao[i]);

            _combinedRMAO?.Dispose();
            _combinedRMAO = TextureFactory.PackTextures(0, 0, textures[0], textures[1], textures[2], textures[3]);
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            _shaderMaterial = new ForwardPBRTerrain(this);
            _shaderMaterial.LoadEffect(Application.Content);
        }

        private static Color GetColor(float value) => new Color(value, value, value, 1);
    }
}
