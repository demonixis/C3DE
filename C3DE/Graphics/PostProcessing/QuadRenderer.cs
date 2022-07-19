
#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace C3DE.Graphics.PostProcessing
{
    /// <summary>
    /// Renders a simple quad to the screen. Uncomment the Vertex / Index buffers to make it a static fullscreen quad. 
    /// The performance effect is barely measurable though and you need to dispose of the buffers when finished!
    /// </summary>
    public sealed class QuadRenderer
    {
        private GraphicsDevice m_GraphicsDevice;
        private readonly VertexPositionNormalTexture[] m_VertexBuffer;
        private readonly short[] m_IndexBuffer;

        public QuadRenderer(GraphicsDevice graphicsDevice)
        {
            m_GraphicsDevice = graphicsDevice;
            m_VertexBuffer = new VertexPositionNormalTexture[4];
            m_VertexBuffer[0] = new VertexPositionNormalTexture(new Vector3(-1, 1, 1), Vector3.Zero, new Vector2(0, 0));
            m_VertexBuffer[1] = new VertexPositionNormalTexture(new Vector3(1, 1, 1), Vector3.Zero, new Vector2(1, 0));
            m_VertexBuffer[2] = new VertexPositionNormalTexture(new Vector3(-1, -1, 1), Vector3.Zero, new Vector2(0, 1));
            m_VertexBuffer[3] = new VertexPositionNormalTexture(new Vector3(1, -1, 1), Vector3.Zero, new Vector2(1, 1));
            m_IndexBuffer = new short[] { 0, 3, 2, 0, 1, 3 };
        }

        public void RenderQuad(Vector2 v1, Vector2 v2)
        {
            m_VertexBuffer[0].Position.X = v1.X;
            m_VertexBuffer[0].Position.Y = v2.Y;
            m_VertexBuffer[1].Position.X = v2.X;
            m_VertexBuffer[1].Position.Y = v2.Y;
            m_VertexBuffer[2].Position.X = v1.X;
            m_VertexBuffer[2].Position.Y = v1.Y;
            m_VertexBuffer[3].Position.X = v2.X;
            m_VertexBuffer[3].Position.Y = v1.Y;
            m_GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, m_VertexBuffer, 0, 4, m_IndexBuffer, 0, 2);
        }

        public void RenderFullscreenQuad() => RenderQuad(Vector2.One * -1, Vector2.One);
    }
}
