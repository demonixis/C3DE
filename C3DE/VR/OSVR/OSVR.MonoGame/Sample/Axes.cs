using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    class Axes
    {
        VertexPositionColor[] vertices = new VertexPositionColor[3];
        VertexBuffer vertexBuffer;
        BasicEffect basicEffect;

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            basicEffect = new BasicEffect(graphicsDevice);
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
        }

        public void Draw(float size, Color color, Matrix view, Matrix world, Matrix projection, GraphicsDevice graphicsDevice)
        {
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;

            vertices[0] = new VertexPositionColor(new Vector3(0f, 0f, -size), color);
            vertices[1] = new VertexPositionColor(new Vector3(size * 0.25f, 0f, 0f), Color.Black);
            vertices[2] = new VertexPositionColor(new Vector3(-size * 0.25f, 0f, 0f), Color.Black);
            
            vertexBuffer.SetData<VertexPositionColor>(vertices);
            graphicsDevice.SetVertexBuffer(vertexBuffer);

            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            graphicsDevice.RasterizerState = rasterizerState;

            foreach(var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }
        }
    }
}
