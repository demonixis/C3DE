using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Geometries
{
    public class TorusGeometry : Geometry
    {
        private float _radiusExterior;
        private float _raduisInterior;
        private int _nbSlices;
        private int _nbSegments;

        public TorusGeometry()
            : this(2, 1, 8, 8)
        {
        }

        public TorusGeometry(float radiusExterior, float radiusInterior, int nbSlices, int nbSegments)
        {
            _radiusExterior = radiusExterior;
            _raduisInterior = radiusInterior;
            _nbSlices = nbSlices;
            _nbSegments = nbSegments;
        }

        protected override void CreateGeometry()
        {
            _nbSegments = Math.Max(3, _nbSegments);
            _nbSlices = Math.Max(3, _nbSlices);

            float invSegments = 1f / _nbSegments, invSlices = 1f / _nbSlices;
            float radSegment = MathHelper.TwoPi * invSegments;
            float radSlice = MathHelper.TwoPi * invSlices;
            bool lines = false;

            int indexCount = 0;
            Vertices = new VertexPositionNormalTexture[(_nbSegments + 1) * (_nbSlices + 1)];
            Indices = new short[_nbSegments * _nbSlices * (lines ? 8 : 6)];

            for (int j = 0; j <= _nbSegments; j++)
            {
                float theta = j * radSegment - MathHelper.PiOver2;
                float cosTheta = (float)Math.Cos(theta), sinTheta = (float)Math.Sin(theta);

                for (int i = 0; i <= _nbSlices; i++)
                {
                    float phi = i * radSlice;
                    float cosPhi = (float)Math.Cos(phi);
                    float sinPhi = (float)Math.Sin(phi);

                    Vector3 position = new Vector3()
                    {
                        X = cosTheta * (_radiusExterior + _raduisInterior * cosPhi),
                        Y = _raduisInterior * sinPhi,
                        Z = sinTheta * (_radiusExterior + _raduisInterior * cosPhi)
                    };
                    Vector3 center = new Vector3()
                    {
                        X = _radiusExterior * cosTheta,
                        Y = 0,
                        Z = _radiusExterior * sinTheta
                    };

                    Vertices[(j * (_nbSlices + 1)) + i] = new VertexPositionNormalTexture()
                    {
                        Position = position,
                        Normal = Vector3.Normalize(position - center),
                        TextureCoordinate = new Vector2(j * invSegments, i * invSegments)
                    };

                    // 0---2
                    // | \ |
                    // 1---3
                    if (j < _nbSegments && i < _nbSlices)
                    {
                        short i0 = (short)((j * (_nbSlices + 1)) + i);
                        short i1 = (short)((j * (_nbSlices + 1)) + i + 1);
                        short i2 = (short)(((j + 1) * (_nbSlices + 1)) + i);
                        short i3 = (short)(((j + 1) * (_nbSlices + 1)) + i + 1);

                        Indices[indexCount++] = i0;
                        Indices[indexCount++] = invertFaces ? i1 : i3;
                        Indices[indexCount++] = invertFaces ? i3 : i1;

                        Indices[indexCount++] = i0;
                        Indices[indexCount++] = invertFaces ? i3 : i2;
                        Indices[indexCount++] = invertFaces ? i2 : i3;
                    }
                }
            }

            ComputeNormals();
        }
    }
}
