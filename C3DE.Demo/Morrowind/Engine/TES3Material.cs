using C3DE;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TES3Unity.Rendering
{
    public enum MatTestMode
    {
        Always = 0,
        Less,
        LEqual,
        Equal,
        GEqual,
        Greater,
        NotEqual,
        Never
    }

    public struct TES3MaterialTextures
    {
        public string mainFilePath;
        public string darkFilePath;
        public string detailFilePath;
        public string glossFilePath;
        public string glowFilePath;
        public string bumpFilePath;
    }

    public struct TES3MaterialProps
    {
        public TES3MaterialTextures textures;
        public bool alphaBlended;
        public byte srcBlendMode;
        public byte dstBlendMode;
        public bool alphaTest;
        public float alphaCutoff;
        public bool zWrite;
        public Color diffuseColor;
        public Color specularColor;
        public Color emissiveColor;
        public float glossiness;
        public float alpha;

        public string GetMaterialName()
        {
            if (!string.IsNullOrEmpty(textures.mainFilePath))
                return textures.mainFilePath;

            return $"{diffuseColor}{specularColor}{emissiveColor}{glossiness}{alpha}";
        }
    }

    /// <summary>
    /// A material container compatible with Universal RP and HDRP.
    /// </summary>
    public sealed class TES3Material
    {
        // Static variables
        private static Dictionary<string, Material> MaterialStore = new Dictionary<string, Material>();

        // Private variables
        private TextureManager m_TextureManager;
        private bool m_GenerateNormals;

        public TES3Material(TextureManager textureManager, bool generateNormal)
        {
            m_TextureManager = textureManager;
            m_GenerateNormals = false; // FIXME generateNormal;
        }

        public Material BuildMaterialFromProperties(TES3MaterialProps mp)
        {
            var name = mp.GetMaterialName();
            if (MaterialStore.ContainsKey(name))
            {
                return MaterialStore[name];
            }

            var material = new StandardMaterial();

            if (mp.alphaBlended)
            {
                material.CutoutEnabled = true;
                material.Cutout = 0.5f;
            }

            if (mp.textures.mainFilePath != null)
            {
                var mainTexture = m_TextureManager.LoadTexture(mp.textures.mainFilePath);
                material.MainTexture = mainTexture;

                if (m_GenerateNormals)
                {
                    var normalMap = TextureManager.CreateNormalMapTexture(mainTexture, 2);
                    material.NormalMap = normalMap;
                }
            }

            MaterialStore.Add(name, material);

            return material;
        }
    }
}
