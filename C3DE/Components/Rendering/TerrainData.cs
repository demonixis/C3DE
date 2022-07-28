using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public sealed class TerrainData
    {
        public float[,] heightmapTexture { get; set; }
        public float[,,] alphamapTextures { get; set; }

        public int heightmapResolution { get; set; }
        public Vector3 heightmapScale { get; set; } = Vector3.One;
        public Vector3 size { get; set; }
        public  float wavingGrassStrength { get; set; }

        public  float wavingGrassAmount { get; set; }
        public  float wavingGrassSpeed { get; set; }
        public Color wavingGrassTint { get; set; }
        public  int detailWidth { get; set; }
        public  int detailHeight { get; set; }
        public  int detailPatchCount { get; set; }
        public  int detailResolution { get; set; }
        public  int detailResolutionPerPatch { get; set; }
        public  int treeInstanceCount { get; set; }
        public int alphamapLayers { get; set; }
        public int alphamapResolution { get; set; }
        public int alphamapWidth => alphamapTextures.GetLength(0);
        public int alphamapHeight => alphamapTextures.GetLength(1);
        public int baseMapResolution { get; set; }
        public int alphamapTextureCount { get; set; }
       
        public TerrainLayer[] terrainLayers { get; set; }

        public void SetHeights(int x, int y, float[,] data)
        {
            heightmapTexture = data;
        }

        public void SetAlphamaps(int x, int y, float[,,] data)
        {
            alphamapTextures = data;
        }
    }
}
