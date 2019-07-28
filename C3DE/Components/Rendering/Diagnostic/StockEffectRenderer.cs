using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public class StockEffectRenderer : Renderer
    {
        public Mesh Mesh { get; set; }
        public Effect Effect { get; set; }

        public override void ComputeBoundingInfos()
        {
        }

        public override void Draw(GraphicsDevice device)
        {
            if (Mesh == null || Effect == null)
                return;

            device.SetVertexBuffer(Mesh.VertexBuffer);
            device.Indices = Mesh.IndexBuffer;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Mesh.Indices.Length / 3);
            }
        }
    }
}
