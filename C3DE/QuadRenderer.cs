using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE
{
    public class QuadRenderer
    {
        private VertexPositionTexture[] _vertices;
        private short[] _indices;

        private Vector3[] VerticePositions = 
        {
            new Vector3(-1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f)
        };

        private Vector2[] VerticeUVs = 
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 1.0f)
        };

        public QuadRenderer()
        {
            _vertices = new VertexPositionTexture[4];

            _vertices[0] = new VertexPositionTexture(new Vector3(-1.0f, 1.0f, 1.0f), new Vector2(0.0f, 0.0f));
            _vertices[1] = new VertexPositionTexture(new Vector3(1.0f, 1.0f, 1.0f), new Vector2(1.0f, 0.0f));
            _vertices[2] = new VertexPositionTexture(new Vector3(-1.0f, -1.0f, 1.0f), new Vector2(0.0f, 1.0f));
            _vertices[3] = new VertexPositionTexture(new Vector3(1.0f, -1.0f, 1.0f), new Vector2(1.0f, 1.0f));

            _indices = new short[] { 0, 3, 2, 0, 1, 3 };
        }

        public void Draw(GraphicsDevice device, Vector2 topLeftCorner, Vector2 bottomRightCorner)
        {
            _vertices[0].Position.X = topLeftCorner.X;
            _vertices[0].Position.Y = bottomRightCorner.Y;
            _vertices[1].Position.X = bottomRightCorner.X;
            _vertices[1].Position.Y = bottomRightCorner.Y;
            _vertices[2].Position.X = topLeftCorner.X;
            _vertices[2].Position.Y = topLeftCorner.Y;
            _vertices[3].Position.X = bottomRightCorner.X;
            _vertices[4].Position.Y = topLeftCorner.Y;

            device.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, _vertices, 0, 4, _indices, 0, 2);
        }

        public void Draw(GraphicsDevice device)
        {
            Draw(device, -Vector2.One, Vector2.One);
        }
    }
}
