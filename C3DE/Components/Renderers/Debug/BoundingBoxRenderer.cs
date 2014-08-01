using C3DE.Components.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Renderers.Debug
{
    public static class BoundingBoxRenderer
    {
        #region Properties

        private static VertexPositionColor[] verts = new VertexPositionColor[8];
        private static short[] indices = new short[]
        {
            0, 1,
            1, 2,
            2, 3,
            3, 0,
            0, 4,
            1, 5,
            2, 6,
            3, 7,
            4, 5,
            5, 6,
            6, 7,
            7, 4,
        };

        private static BasicEffect effect;

        #endregion

        public static void Draw(BoundingBox box, Camera camera, Color color)
        {
            if (effect == null)
            {
                effect = new BasicEffect(Application.GraphicsDevice);
                effect.VertexColorEnabled = true;
                effect.LightingEnabled = false;
            }

            Vector3[] corners = box.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                verts[i].Position = corners[i];
                verts[i].Color = color;
            }

            effect.View = camera.view;
            effect.Projection = camera.projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Application.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, verts, 0, 8, indices, 0, indices.Length / 2);
            }
        }
    }
}
