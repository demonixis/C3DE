using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public sealed class TerrainLayer
    {
        public Texture2D diffuseTexture { get; set; }
        public Texture2D normalMapTexture { get; set; }
        public Texture2D maskMapTexture { get; set; }
        public Vector2 tileSize { get; set; }
        public Vector2 tileOffset { get; set; }
        public Color specular { get; set; }
        public float metallic { get; set; }
        public float smoothness { get; set; }
        public float normalScale { get; set; }
        public Vector4 diffuseRemapMin { get; set; }
        public Vector4 diffuseRemapMax { get; set; }
        public Vector4 maskMapRemapMin { get; set; }
        public Vector4 maskMapRemapMax { get; set; }
    }
}
