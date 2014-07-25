using Microsoft.Xna.Framework;

namespace C3DE.Components
{
    public class Camera : Component
    {
        internal Matrix projection;
        internal Matrix view;

        public Vector3 WorldToScreenPoint(Vector3 position)
        {
           return App.GraphicsDevice.Viewport.Project(position, projection, view, Matrix.Identity);
        }

        /// <summary>
        /// Get 3D world position of from the 2D position (on screen)
        /// </summary>
        /// <param name="camera">Camera to use</param>
        /// <param name="position">Position on world</param>
        /// <returns>Position on 3D world</returns>
        public Vector3 ScreenToWorld(Vector3 position)
        {
            return App.GraphicsDevice.Viewport.Unproject(position, projection, view, Matrix.Identity);
        }

        public Ray GetRay(Vector2 position)
        {
            Vector3 nearPoint = new Vector3(position, 0);
            Vector3 farPoint = new Vector3(position, 1);

            nearPoint = App.GraphicsDevice.Viewport.Unproject(nearPoint, projection, view, Matrix.Identity);
            farPoint = App.GraphicsDevice.Viewport.Unproject(farPoint, projection, view, Matrix.Identity);

            // Get the direction
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }
    }
}
