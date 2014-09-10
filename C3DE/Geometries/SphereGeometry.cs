using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Geometries
{
    public class SphereGeometry : Geometry
    {
        private float _radius;
        private int _tessellationLevel;

        public float Radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        public int TessellationLevel
        {
            get { return _tessellationLevel; }
            set { _tessellationLevel = value; }
        }

       public SphereGeometry()
           : this(1, 8)
        {
        }

        public SphereGeometry(float radius = 1.0f, int tessellation = 8)
        {
            _radius = radius;
            _tessellationLevel = tessellation;
        }

        protected override void CreateGeometry()
        {
            if (_radius < 0)
                _radius *= -1;

            _tessellationLevel = Math.Max(4, _tessellationLevel);
            _tessellationLevel += (_tessellationLevel % 2);

            int vertexCount = 0;
            int indexCount = 0;

            Vertices = new VertexPositionNormalTexture[((_tessellationLevel / 2) * (_tessellationLevel - 1)) + 1];
            Indices = new short[(((_tessellationLevel / 2) - 2) * (_tessellationLevel + 1) * 6) + (6 * (_tessellationLevel + 1))];

            for (int j = 0; j <= _tessellationLevel / 2; j++)
            {
                float theta = j * MathHelper.TwoPi / _tessellationLevel - MathHelper.PiOver2;

                for (int i = 0; i <= _tessellationLevel; i++)
                {
                    float phi = i * MathHelper.TwoPi / _tessellationLevel;

                    Vertices[vertexCount++] = new VertexPositionNormalTexture()
                    {
                        Position = new Vector3()
                        {
                            X = (_radius * (float)(Math.Cos(theta) * Math.Cos(phi))),
                            Y = (_radius * (float)(Math.Sin(theta))),
                            Z = (_radius * (float)(Math.Cos(theta) * Math.Sin(phi)))
                        },
                        
                        TextureCoordinate = new Vector2()
                        {
                            X = ((float)i / (float)_tessellationLevel),
                            Y = ((float)2 * (float)j / (float)_tessellationLevel)
                        }
                    };

                    if (j == 0)
                    {
                        // bottom cap
                        for (i = 0; i <= _tessellationLevel; i++)
                        {
                            short i0 = 0;
                            short i1 = (short)((i % _tessellationLevel) + 1);
                            short i2 = (short)i;

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i2 : i1;
                            Indices[indexCount++] = invertFaces ? i1 : i2;
                        }
                    }
                    else if (j < _tessellationLevel / 2 - 1)
                    {
                        // middle area
                        short i0 = (short)(vertexCount - 1);
                        short i1 = (short)vertexCount;
                        short i2 = (short)(vertexCount + _tessellationLevel);
                        short i3 = (short)(vertexCount + _tessellationLevel + 1);

                        Indices[indexCount++] = i0;
                        Indices[indexCount++] = invertFaces ? i2 : i1;
                        Indices[indexCount++] = invertFaces ? i1 : i2;

                        Indices[indexCount++] = i1;
                        Indices[indexCount++] = invertFaces ? i2 : i3;
                        Indices[indexCount++] = invertFaces ? i3 : i2;
                    }
                    else if (j == _tessellationLevel / 2)
                    {
                        // top cap
                        for (i = 0; i <= _tessellationLevel; i++)
                        {
                            short i0 = (short)(vertexCount - 1);
                            short i1 = (short)((vertexCount - 1) - ((i % _tessellationLevel) + 1));
                            short i2 = (short)((vertexCount - 1) - i);

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i2 : i1;
                            Indices[indexCount++] = invertFaces ? i1 : i2;
                        }
                    }
                }
            }

            ComputeNormals();
        }
    }
}
