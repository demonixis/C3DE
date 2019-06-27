
#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace C3DE.Graphics.PostProcessing
{
    public class FullScreenQuadRenderer : IDisposable
    {
        //buffers for rendering the quad
        private readonly IndexBuffer _indexBuffer;
        private readonly VertexBuffer _vertexBuffer;

        public FullScreenQuadRenderer(GraphicsDevice graphics)
        {
            _vertexBuffer = new VertexBuffer(graphics, FullScreenQuadVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(new[]
            {
                new FullScreenQuadVertex(-1, 1),
                new FullScreenQuadVertex(1, 1),
                new FullScreenQuadVertex(-1, -1),
                new FullScreenQuadVertex(1, -1)
            });

            _indexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            _indexBuffer.SetData(new[] { 0, 3, 2, 0, 1, 3 });
        }

        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void RenderFullscreenQuad(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Indices = _indexBuffer;
            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4);
        }

        private struct FullScreenQuadVertex
        {
            public const int SizeInBytes = 8;
            public Vector2 Position;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0)
            );

            public FullScreenQuadVertex(Vector2 position)
            {
                Position = position;
            }

            public FullScreenQuadVertex(float x, float y)
            {
                Position.X = x;
                Position.Y = y;
            }
        }

        public void Dispose()
        {
            _indexBuffer?.Dispose();
            _vertexBuffer?.Dispose();
        }
    }
}
