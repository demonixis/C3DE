using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE.Geometries
{
    public class SphereGeometry : Geometry
    {
        [DataMember]
        public float Radius { get; set; }

        [DataMember]
        public int TessellationLevel { get; set; }

       public SphereGeometry()
           : this(1, 8)
        {
        }

        public SphereGeometry(float radius = 1.0f, int tessellation = 8)
        {
            Radius = radius;
            TessellationLevel = tessellation;
        }

        protected override void CreateGeometry()
        {
            if (Radius < 0)
                Radius *= -1;

            TessellationLevel = Math.Max(4, TessellationLevel);
            TessellationLevel += (TessellationLevel % 2);

            int vertexCount = 0;
            int indexCount = 0;

            Vertices = new VertexPositionNormalTexture[((TessellationLevel / 2) * (TessellationLevel - 1)) + 1];
            Indices = new ushort[(((TessellationLevel / 2) - 2) * (TessellationLevel + 1) * 6) + (6 * (TessellationLevel + 1))];

            for (int j = 0; j <= TessellationLevel / 2; j++)
            {
                float theta = j * MathHelper.TwoPi / TessellationLevel - MathHelper.PiOver2;

                for (int i = 0; i <= TessellationLevel; i++)
                {
                    float phi = i * MathHelper.TwoPi / TessellationLevel;

                    Vertices[vertexCount++] = new VertexPositionNormalTexture()
                    {
                        Position = new Vector3()
                        {
                            X = (Radius * (float)(Math.Cos(theta) * Math.Cos(phi))),
                            Y = (Radius * (float)(Math.Sin(theta))),
                            Z = (Radius * (float)(Math.Cos(theta) * Math.Sin(phi)))
                        },
                        
                        TextureCoordinate = new Vector2()
                        {
                            X = ((float)i / (float)TessellationLevel),
                            Y = ((float)2 * (float)j / (float)TessellationLevel)
                        }
                    };

                    if (j == 0)
                    {
                        // bottom cap
                        for (i = 0; i <= TessellationLevel; i++)
                        {
                            ushort i0 = 0;
                            ushort i1 = (ushort)((i % TessellationLevel) + 1);
                            ushort i2 = (ushort)i;

                            Indices[indexCount++] = i0;
                            Indices[indexCount++] = invertFaces ? i2 : i1;
                            Indices[indexCount++] = invertFaces ? i1 : i2;
                        }
                    }
                    else if (j < TessellationLevel / 2 - 1)
                    {
                        // middle area
                        ushort i0 = (ushort)(vertexCount - 1);
                        ushort i1 = (ushort)vertexCount;
                        ushort i2 = (ushort)(vertexCount + TessellationLevel);
                        ushort i3 = (ushort)(vertexCount + TessellationLevel + 1);

                        Indices[indexCount++] = i0;
                        Indices[indexCount++] = invertFaces ? i2 : i1;
                        Indices[indexCount++] = invertFaces ? i1 : i2;

                        Indices[indexCount++] = i1;
                        Indices[indexCount++] = invertFaces ? i2 : i3;
                        Indices[indexCount++] = invertFaces ? i3 : i2;
                    }
                    else if (j == TessellationLevel / 2)
                    {
                        // top cap
                        for (i = 0; i <= TessellationLevel; i++)
                        {
                            ushort i0 = (ushort)(vertexCount - 1);
                            ushort i1 = (ushort)((vertexCount - 1) - ((i % TessellationLevel) + 1));
                            ushort i2 = (ushort)((vertexCount - 1) - i);

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
