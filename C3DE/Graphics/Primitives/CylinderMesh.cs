using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Primitives
{
    [DataContract]
    public class CylinderMesh : Mesh
    {
        [DataMember]
        public Vector3 StartPosition { get; set; }

        [DataMember]
        public Vector3 EndPosition { get; set; }

        [DataMember]
        public float StartRadius { get; set; }

        [DataMember]
        public float EndRadius { get; set; }

        [DataMember]
        public int NbSegments { get; set; }

        [DataMember]
        public int NbSlices { get; set; }

        private static Random random = new Random();

        public CylinderMesh()
            : this(new Vector3(0, 0, 0), new Vector3(0, 1, 0))
        {
        }

        public CylinderMesh(Vector3 startPosition, Vector3 endPosition, float startRadius = 1, float endRadius = 1, int nbSegments = 8, int nbSlices = 8)
            :  base()
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            StartRadius = startRadius;
            EndRadius = endRadius;
            NbSegments = nbSegments;
            NbSlices = nbSlices;
        }

        protected override void CreateGeometry()
        {
            NbSegments = Math.Max(1, NbSegments);
            NbSlices = Math.Max(3, NbSlices);

            // this vector should not be between start and end
            Vector3 p = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());

            // derive two points on the plane formed by [end - start]
            Vector3 r = Vector3.Cross(p - StartPosition, EndPosition - StartPosition);
            Vector3 s = Vector3.Cross(r, EndPosition - StartPosition);
            r.Normalize();
            s.Normalize();

            int vertexCount = 0, indexCount = 0;
            float invSegments = 1f / (float)NbSegments;
            float invSlices = 1f / (float)NbSlices;

            Vertices = new VertexPositionNormalTexture[((NbSegments + 1) * (NbSlices + 1)) + 2];
            Indices = new ushort[(NbSlices + (NbSlices * NbSegments)) * 6];

            for (int j = 0; j <= NbSegments; j++)
            {
                Vector3 center = Vector3.Lerp(StartPosition, EndPosition, j * invSegments);
                float radius = MathHelper.Lerp(StartRadius, EndRadius, j * invSegments);

                if (j == 0)
                {
                    Vertices[vertexCount++] = new VertexPositionNormalTexture()
                    {
                        Position = center,
                        TextureCoordinate = new Vector2(0.5f, (float)j * invSegments)
                    };
                }

                for (int i = 0; i <= NbSlices; i++)
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

                    if (i < NbSlices)
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

                        if (j == NbSegments)
                        {   
                            // end cap - i0 is always the center point on end cap
                            ushort i0 = (ushort)((vRef + NbSlices + 2) - (vRef % (NbSlices + 1)));
                            ushort i1 = (ushort)(vRef);
                            ushort i2 = (ushort)(vRef + 1);

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i2 : i1;
                            Indices[indexCount++] = invertFaces ? i1 : i2;
                        }

                        if (j < NbSegments)
                        {   
                            // middle area
                            ushort i0 = (ushort)(vRef);
                            ushort i1 = (ushort)(vRef + 1);
                            ushort i2 = (ushort)(vRef + NbSlices + 2);
                            ushort i3 = (ushort)(vRef + NbSlices + 1);

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i2 : i1;
                            Indices[indexCount++] = invertFaces ? i1 : i2;

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i3 : i2;
                            Indices[indexCount++] = invertFaces ? i2 : i3;
                        }
                    }
                }

                if (j == NbSegments)
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
