using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Geometries
{
    [DataContract]
    public class PyramidGeometry : Geometry
    {
        protected override void CreateGeometry()
        {
            Vector2 topRight = new Vector2(1.0f, 0.0f);
            Vector2 bottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 bottomRight = new Vector2(1.0f, 1.0f);

            Vector3 normal = Vector3.Up;

            Vertices = new VertexPositionNormalTexture[12];
            Vertices[0] = new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, 1.0f), normal, bottomRight);
            Vertices[1] = new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, 1.0f), normal, bottomLeft);
            Vertices[2] = new VertexPositionNormalTexture(new Vector3(0.0f, 1.0f, 0.0f), normal, topRight);

            Vertices[3] = new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, -1.0f), normal, bottomRight);
            Vertices[4] = new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, 1.0f), normal, bottomLeft);
            Vertices[5] = new VertexPositionNormalTexture(new Vector3(0.0f, 1.0f, 0.0f), normal, topRight);

            Vertices[6] = new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, -1.0f), normal, bottomRight);
            Vertices[7] = new VertexPositionNormalTexture(new Vector3(1.0f, -1.0f, -1.0f), normal, bottomLeft);
            Vertices[8] = new VertexPositionNormalTexture(new Vector3(0.0f, 1.0f, 0.0f), normal, topRight);

            Vertices[9] = new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, 1.0f), normal, bottomRight);
            Vertices[10] = new VertexPositionNormalTexture(new Vector3(-1.0f, -1.0f, -1.0f), normal, bottomLeft);
            Vertices[11] = new VertexPositionNormalTexture(new Vector3(0.0f, 1.0f, 0.0f), normal, topRight);

            Indices = new ushort[] 
            { 
                0, 1, 2, 
                3, 4, 5, 
                6, 7, 8,
                9, 10, 11 
            };

            ComputeNormals();
        }
    }
}
