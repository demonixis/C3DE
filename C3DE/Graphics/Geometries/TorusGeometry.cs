using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Geometries
{
    [DataContract]
    public class TorusGeometry : Geometry
    {
        [DataMember]
        public float RadiusExterior { get; set; }

        [DataMember]
        public float RaduisInterior { get; set; }

        [DataMember]
        public int NbSlices { get; set; }

        [DataMember]
        public int NbSegments { get; set; }

        public TorusGeometry()
            : this(2, 1, 8, 8)
        {
        }

        public TorusGeometry(float radiusExterior, float radiusInterior, int nbSlices, int nbSegments)
        {
            RadiusExterior = radiusExterior;
            RaduisInterior = radiusInterior;
            NbSlices = nbSlices;
            NbSegments = nbSegments;
        }

        protected override void CreateGeometry()
        {
            NbSegments = Math.Max(3, NbSegments);
            NbSlices = Math.Max(3, NbSlices);

            float invSegments = 1f / NbSegments, invSlices = 1f / NbSlices;
            float radSegment = MathHelper.TwoPi * invSegments;
            float radSlice = MathHelper.TwoPi * invSlices;
            bool lines = false;

            int indexCount = 0;
            Vertices = new VertexPositionNormalTexture[(NbSegments + 1) * (NbSlices + 1)];
            Indices = new ushort[NbSegments * NbSlices * (lines ? 8 : 6)];

            for (int j = 0; j <= NbSegments; j++)
            {
                float theta = j * radSegment - MathHelper.PiOver2;
                float cosTheta = (float)Math.Cos(theta), sinTheta = (float)Math.Sin(theta);

                for (int i = 0; i <= NbSlices; i++)
                {
                    float phi = i * radSlice;
                    float cosPhi = (float)Math.Cos(phi);
                    float sinPhi = (float)Math.Sin(phi);

                    Vector3 position = new Vector3()
                    {
                        X = cosTheta * (RadiusExterior + RaduisInterior * cosPhi),
                        Y = RaduisInterior * sinPhi,
                        Z = sinTheta * (RadiusExterior + RaduisInterior * cosPhi)
                    };
                    Vector3 center = new Vector3()
                    {
                        X = RadiusExterior * cosTheta,
                        Y = 0,
                        Z = RadiusExterior * sinTheta
                    };

                    Vertices[(j * (NbSlices + 1)) + i] = new VertexPositionNormalTexture()
                    {
                        Position = position,
                        Normal = Vector3.Normalize(position - center),
                        TextureCoordinate = new Vector2(j * invSegments, i * invSegments)
                    };

                    // 0---2
                    // | \ |
                    // 1---3
                    if (j < NbSegments && i < NbSlices)
                    {
                        ushort i0 = (ushort)((j * (NbSlices + 1)) + i);
                        ushort i1 = (ushort)((j * (NbSlices + 1)) + i + 1);
                        ushort i2 = (ushort)(((j + 1) * (NbSlices + 1)) + i);
                        ushort i3 = (ushort)(((j + 1) * (NbSlices + 1)) + i + 1);

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
