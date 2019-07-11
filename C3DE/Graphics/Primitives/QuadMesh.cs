using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Primitives
{
    public class QuadMesh : Mesh
    {
        public Vector3 Direction { get; set; } = new Vector3(1, 1, 0);

        protected override void CreateGeometry()
        {
            var position = new Vector3[4]
            {
                new Vector3(-0.5f, 0.5f, 0.0f),
                new Vector3(0.5f, 0.5f, 0.0f),
                new Vector3(0.5f, -0.5f, 0.0f),
                new Vector3(-0.5f, -0.5f, 0.0f)
            };

            for (var i = 0; i < position.Length; i++)
                position[i] *= Direction;

            var uvs = new Vector2[4]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.5f, 0.0f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.0f, 0.5f)
            };

            Vertices = new VertexPositionNormalTexture[4];

            for (int i = 0; i < 4; i++)
            {
                Vertices[i].Position = position[i];
                Vertices[i].TextureCoordinate = uvs[i];
                Vertices[i].Normal = Vector3.Forward;
            }

            Indices = new ushort[] { 0, 1, 2, 0, 2, 3 };
        }
    }
}
