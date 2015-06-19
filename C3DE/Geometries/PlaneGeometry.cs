using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Geometries
{
    public class PlaneGeometry : Geometry
    {
        protected override void CreateGeometry()
        {
            var position = new Vector3[4]
            {
                new Vector3(-1.0f, 0.0f, 1.0f),
                new Vector3(-1.0f, 0.0f, -1.0f),
                new Vector3(1.0f, 0.0f, -1.0f),
                new Vector3(1.0f, 0.0f, 1.0f)
            };

            var uvs = new Vector2[4]
            {
                new Vector2(0.0f, 1.0f),
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f)
            };

            Vertices = new VertexPositionNormalTexture[4];

            for (int i = 0; i < 4; i++)
            {
                Vertices[i].Position = position[i];
                Vertices[i].TextureCoordinate = uvs[i];
                Vertices[i].Normal = Vector3.Up;
            }

            Indices = new ushort[] { 0, 1, 2, 0, 2, 3 };
        }
    }
}
