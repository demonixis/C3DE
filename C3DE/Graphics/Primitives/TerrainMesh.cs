using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Primitives
{
    public class TerrainMesh : Mesh
    {
        public int HeightmapSize { get; set; } = 65;
        public float[,] Data { get; set; }
        public bool OriginAtCenter { get; set; } = true;
        public bool ReverseHeightmap { get; set; } = false;

        public TerrainMesh()
            : base()
        {
            Data = new float[HeightmapSize, HeightmapSize];

            for (int x = 0; x < HeightmapSize; x++)
            {
                for (int z = 0; z < HeightmapSize; z++)
                    Data[x, z] = 0.0f;
            }
        }


        protected override void CreateGeometry()
        {
            Vertices = new VertexPositionNormalTexture[HeightmapSize * HeightmapSize];

            var xx = 0;
            if (OriginAtCenter)
                xx = -HeightmapSize / 2;

            for (int x = 0; x < HeightmapSize; x++)
            {
                var zz = 0;
                if (OriginAtCenter)
                    zz = -HeightmapSize / 2;

                for (int z = 0; z < HeightmapSize; z++)
                {
                    Vertices[x + z * HeightmapSize].Position = new Vector3(ReverseHeightmap ? zz : xx, Data[x, z], ReverseHeightmap ? xx : zz);

                    Vertices[x + z * HeightmapSize].TextureCoordinate = new Vector2(
                        ((float)x / (float)HeightmapSize),
                        ((float)z / (float)HeightmapSize));

                    Vertices[x + z * HeightmapSize].Normal = Vector3.Up;

                    zz++;
                }

                xx++;
            }

            Indices = new ushort[(HeightmapSize - 1) * (HeightmapSize - 1) * 6];

            int counter = 0;
            ushort lowerLeft = 0;
            ushort lowerRight = 0;
            ushort topLeft = 0;
            ushort topRight = 0;

            for (int x = 0; x < HeightmapSize - 1; x++)
            {
                for (int y = 0; y < HeightmapSize - 1; y++)
                {
                    lowerLeft = (ushort)(x + y * HeightmapSize);
                    lowerRight = (ushort)((x + 1) + y * HeightmapSize);
                    topLeft = (ushort)(x + (y + 1) * HeightmapSize);
                    topRight = (ushort)((x + 1) + (y + 1) * HeightmapSize);

                    Indices[counter++] = topLeft;
                    Indices[counter++] = lowerLeft;
                    Indices[counter++] = lowerRight;
                    Indices[counter++] = topLeft;
                    Indices[counter++] = lowerRight;
                    Indices[counter++] = topRight;
                }
            }

        }

        public override void Build()
        {
            if (_vertices?.Length == 0) return;

            Dispose();
            CreateGeometry();
            ApplyParameters();
            ComputeNormals();
            CreateBuffers(Application.GraphicsDevice);
            Built = true;
        }
    }
}
