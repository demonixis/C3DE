using C3DE.Components.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Renderers.Debug
{
    public static class RayRenderer
    {
        private static BasicEffect effect = null;
        private static VertexPositionColor[] verts = new VertexPositionColor[2];

        static VertexPositionColor[] arrowVerts = {
            new VertexPositionColor(Vector3.Zero, Color.White),
            new VertexPositionColor(new Vector3(.5f, 0f, -.5f), Color.White),
            new VertexPositionColor(new Vector3(-.5f, 0f, -.5f), Color.White),
            new VertexPositionColor(new Vector3(0f, .5f, -.5f), Color.White),
            new VertexPositionColor(new Vector3(0f, -.5f, -.5f), Color.White),
        };

        static int[] arrowIndexs = {
            0, 1,
            0, 2,
            0, 3,
            0, 4,
        };

        public static void Draw(Ray ray, float length, Camera camera, Color color)
        {
            if (effect == null)
            {
                effect = new BasicEffect(App.GraphicsDevice);
                effect.VertexColorEnabled = false;
                effect.LightingEnabled = false;
            }

            verts[0] = new VertexPositionColor(ray.Position, Color.White);
            verts[1] = new VertexPositionColor(ray.Position + (ray.Direction * length), Color.White);

            effect.DiffuseColor = color.ToVector3();
            effect.Alpha = (float)color.A / 255f;

            effect.World = Matrix.Identity;
            effect.View = camera.view;
            effect.Projection = camera.projection;

            App.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.None };

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                App.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts, 0, 1);

                effect.World = Matrix.Invert(Matrix.CreateLookAt(
                    verts[1].Position,
                    verts[0].Position,
                    (ray.Direction != Vector3.Up) ? Vector3.Up : Vector3.Left));

                App.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, arrowVerts, 0, 5, arrowIndexs, 0, 4);
            }

            App.GraphicsDevice.RasterizerState = new RasterizerState() { CullMode = CullMode.CullClockwiseFace };
        }
    }
}
