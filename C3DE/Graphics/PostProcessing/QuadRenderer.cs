
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
        private readonly VertexPositionTexture[] _vertexBuffer;
        private readonly short[] _indexBuffer;

        public QuadRenderer(GraphicsDevice graphicsDevice)
        {
            _vertexBuffer = new VertexPositionTexture[4];
            _vertexBuffer[0] = new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0, 0));
            _vertexBuffer[1] = new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0));
            _vertexBuffer[2] = new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0, 1));
            _vertexBuffer[3] = new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 1));
            _indexBuffer = new short[] { 0, 3, 2, 0, 1, 3 };
        }

        public void RenderQuad(GraphicsDevice device, Vector2 v1, Vector2 v2)
        {
            _vertexBuffer[0].Position.X = v1.X;
            _vertexBuffer[0].Position.Y = v2.Y;
            _vertexBuffer[1].Position.X = v2.X;
            _vertexBuffer[1].Position.Y = v2.Y;
            _vertexBuffer[2].Position.X = v1.X;
            _vertexBuffer[2].Position.Y = v1.Y;
            _vertexBuffer[3].Position.X = v2.X;
            _vertexBuffer[3].Position.Y = v1.Y;
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertexBuffer, 0, 4, _indexBuffer, 0, 2);
        }

        public void RenderFullscreenQuad(GraphicsDevice device) => RenderQuad(device, Vector2.One * -1, Vector2.One);

        public void RenderQuadForEye(GraphicsDevice device, bool leftEye)
        {
            var left = new Vector2(-1.0f, -1.0f);
            var right = new Vector2(0.0f, 1.0f);

            if (!leftEye)
            {
                left.X = 0.0f;
                right.X = 1.0f;
            }

            RenderQuad(device, left, right);
        }
    }
}
