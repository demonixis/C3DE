
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
            FullScreenQuadVertex[] vertexBufferTemp = new FullScreenQuadVertex[4];
            vertexBufferTemp[0] = new FullScreenQuadVertex(new Vector2(-1, 1) );
            vertexBufferTemp[1] = new FullScreenQuadVertex(new Vector2(1, 1) );
            vertexBufferTemp[2] = new FullScreenQuadVertex(new Vector2(-1, -1) );
            vertexBufferTemp[3] = new FullScreenQuadVertex(new Vector2(1, -1));
            short[] indexBufferTemp = new short[] { 0, 3, 2, 0, 1, 3 };

            _vertexBuffer = new VertexBuffer(graphics, FullScreenQuadVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
            _indexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);

            _vertexBuffer.SetData(vertexBufferTemp);
            _indexBuffer.SetData(indexBufferTemp);
        }

        /// </summary>
        /// <param name="graphicsDevice"></param>
        public void RenderFullscreenQuad(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Indices = _indexBuffer;
            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.DrawIndexedPrimitives
                (PrimitiveType.TriangleList, 0,0,4);
        }

        private struct FullScreenQuadVertex
        {
            // Stores the starting position of the particle.
            // ReSharper disable once NotAccessedField.Local
            // ReSharper disable once MemberCanBePrivate.Local
            public Vector2 Position;

            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector2,
                    VertexElementUsage.Position, 0)
            );

            public FullScreenQuadVertex(Vector2 position)
            {
                Position = position;
            }

            // ReSharper disable once UnusedMember.Local
            public const int SizeInBytes = 8;
        }

        public void Dispose()
        {
            _indexBuffer?.Dispose();
            _vertexBuffer?.Dispose();
        }
    }
}
