using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Geometries
{
    public class CylinderGeometry : Geometry
    {
        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _startRadius;
        private float _endRadius;
        private int _nbSegments;
        private int _nbSlices;
        private static Random random = new Random();

        public CylinderGeometry()
            : this(new Vector3(0, 0, 0), new Vector3(0, 1, 0))
        {
        }

        public CylinderGeometry(Vector3 startPosition, Vector3 endPosition, float startRadius = 1, float endRadius = 1, int nbSegments = 8, int nbSlices = 8)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;
            _startRadius = startRadius;
            _endRadius = endRadius;
            _nbSegments = nbSegments;
            _nbSlices = nbSlices;
        }

        protected override void CreateGeometry()
        {
            _nbSegments = Math.Max(1, _nbSegments);
            _nbSlices = Math.Max(3, _nbSlices);

            // this vector should not be between start and end
            Vector3 p = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            // derive two points on the plane formed by [end - start]
            Vector3 r = Vector3.Cross(p - _startPosition, _endPosition - _startPosition);
            Vector3 s = Vector3.Cross(r, _endPosition - _startPosition);
            r.Normalize();
            s.Normalize();

            int vertexCount = 0, indexCount = 0;
            float invSegments = 1f / (float)_nbSegments;
            float invSlices = 1f / (float)_nbSlices;

            Vertices = new VertexPositionNormalTexture[((_nbSegments + 1) * (_nbSlices + 1)) + 2];
            Indices = new ushort[(_nbSlices + (_nbSlices * _nbSegments)) * 6];

            for (int j = 0; j <= _nbSegments; j++)
            {
                Vector3 center = Vector3.Lerp(_startPosition, _endPosition, j * invSegments);
                float radius = MathHelper.Lerp(_startRadius, _endRadius, j * invSegments);

                if (j == 0)
                {
                    Vertices[vertexCount++] = new VertexPositionNormalTexture()
                    {
                        Position = center,
                        TextureCoordinate = new Vector2(0.5f, (float)j * invSegments)
                    };
                }

                for (int i = 0; i <= _nbSlices; i++)
                {
                    float theta = i * MathHelper.TwoPi * invSlices;
                    float rCosTheta = radius * (float)Math.Cos(theta);
                    float rSinTheta = radius * (float)Math.Sin(theta);

                    Vertices[vertexCount++] = new VertexPositionNormalTexture()
                    {
                        Position = new Vector3()
                        {
                            X = (center.X + rCosTheta * r.X + rSinTheta * s.X),
                            Y = (center.Y + rCosTheta * r.Y + rSinTheta * s.Y),
                            Z = (center.Z + rCosTheta * r.Z + rSinTheta * s.Z)
                        },
                        TextureCoordinate = new Vector2()
                        {
                            X = i * invSlices,
                            Y = j * invSegments
                        }
                    };

                    if (i < _nbSlices)
                    {
                        // just an alias to assist with think of each vertex that's
                        //  iterated in here as the bottom right corner of a triangle
                        int vRef = vertexCount - 1;

                        if (j == 0)
                        {   
                            // start cap - i0 is always center point on start cap
                            ushort i0 = 0;
                            ushort i1 = (ushort)(vRef + 1);
                            ushort i2 = (ushort)(vRef);

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i2 : i1;
                            Indices[indexCount++] = invertFaces ? i1 : i2;
                        }
                        if (j == _nbSegments)
                        {   
                            // end cap - i0 is always the center point on end cap
                            ushort i0 = (ushort)((vRef + _nbSlices + 2) - (vRef % (_nbSlices + 1)));
                            ushort i1 = (ushort)(vRef);
                            ushort i2 = (ushort)(vRef + 1);

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i2 : i1;
                            Indices[indexCount++] = invertFaces ? i1 : i2;
                        }

                        if (j < _nbSegments)
                        {   
                            // middle area
                            ushort i0 = (ushort)(vRef);
                            ushort i1 = (ushort)(vRef + 1);
                            ushort i2 = (ushort)(vRef + _nbSlices + 2);
                            ushort i3 = (ushort)(vRef + _nbSlices + 1);

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i2 : i1;
                            Indices[indexCount++] = invertFaces ? i1 : i2;

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i3 : i2;
                            Indices[indexCount++] = invertFaces ? i2 : i3;
                        }
                    }
                }

                if (j == _nbSegments)
                {
                    Vertices[vertexCount++] = new VertexPositionNormalTexture()
                    {
                        Position = center,
                        TextureCoordinate = new Vector2(0.5f, (float)j * invSegments)
                    };
                }
            }

            ComputeNormals();
        }
    }
}
