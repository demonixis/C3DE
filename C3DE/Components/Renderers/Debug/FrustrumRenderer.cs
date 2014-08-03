using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Renderers.Debug
{
    public static class BoundingFrustumRenderer
    {
        #region Private declarations

        private static BasicEffect effect;
        private static VertexPositionColor[] vertices = new VertexPositionColor[8];
        private static int[] indices = new int[]
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

        #endregion

        /// <summary>
        /// Draw a bounding frustrum representation
        /// </summary>
        /// <param name="frustum">Frustrum</param>
        /// <param name="camera">Camera</param>
        /// <param name="color">Color</param>
        public static void Draw(BoundingFrustum frustum, Camera camera, Color color)
        {
            if (effect == null)
            {
                effect = new BasicEffect(Application.GraphicsDevice);
                effect.VertexColorEnabled = true;
                effect.LightingEnabled = false;
            }

            Vector3[] corners = frustum.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                vertices[i].Position = corners[i];
                vertices[i].Color = color;
            }

            effect.View = camera.view;
            effect.Projection = camera.projection;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Application.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, 8, indices, 0, indices.Length / 2);
            }
        }
    }
}
