using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Geometries
{
    public class TerrainGeometry : Geometry
    {
        private int _width;
        private int _height;
        private int _depth;
        private float[,] _data;

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public int Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        public float[,] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public TerrainGeometry(int width = 25, int depth = 25, float scale = 10.0f, bool isDynamic = false)
            : base()
        {
            _width = width;
            _height = 0;
            _depth = depth;
            size = new Vector3(scale);

            _data = new float[_width, Depth];

            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _depth; z++)
                    Data[x, z] = 0.0f;
            }

            useDynamicBuffers = isDynamic;
        }

        protected override void CreateGeometry()
        {
            Vertices = new VertexPositionNormalTexture[_width * _depth];

            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    Vertices[x + z * _width].Position = new Vector3(x, _data[x, z], z);

                    Vertices[x + z * _width].TextureCoordinate = new Vector2(
                        ((float)x / (float)_width),
                        ((float)z / (float)_depth));

                    Vertices[x + z * _width].Normal = Vector3.Up;
                }
            }

            Indices = new ushort[(_width - 1) * (_depth - 1) * 6];

            int counter = 0;
            ushort lowerLeft = 0;
            ushort lowerRight = 0;
            ushort topLeft = 0;
            ushort topRight = 0;

            for (int x = 0; x < _width - 1; x++)
            {
                for (int y = 0; y < _depth - 1; y++)
                {
                    lowerLeft = (ushort)(x + y * _width);
                    lowerRight = (ushort)((x + 1) + y * _width);
                    topLeft = (ushort)(x + (y + 1) * _width);
                    topRight = (ushort)((x + 1) + (y + 1) * _width);

                    Indices[counter++] = topLeft;
                    Indices[counter++] = lowerLeft;
                    Indices[counter++] = lowerRight;
                    Indices[counter++] = topLeft;
                    Indices[counter++] = lowerRight;
                    Indices[counter++] = topRight;
                }
            }

            ComputeNormals();
        }
    }
}
