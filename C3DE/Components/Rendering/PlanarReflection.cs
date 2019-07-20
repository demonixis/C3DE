using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public class PlanarReflection : Component
    {
        internal RenderTarget2D _reflectionRT;
        internal Matrix _reflectionViewMatrix;
        internal Vector3 _reflectionCameraPosition;
        private RenderTargetBinding[] _previousRT;

        public Texture2D ReflectionMap => (Texture2D)_reflectionRT;
        public bool IsReady { get; private set; }

        public void Initialize(GraphicsDevice graphics, int renderTargetSize)
        {
            GenerateRT(graphics, renderTargetSize);
            IsReady = true;
        }

        public void GenerateRT(GraphicsDevice graphics, int size)
        {
            if (_reflectionRT != null && !_reflectionRT.IsDisposed)
                _reflectionRT.Dispose();

            _reflectionRT = new RenderTarget2D(graphics, size, size, false, SurfaceFormat.Color, DepthFormat.Depth16);
        }

        public void BeginDraw(GraphicsDevice device, ref Vector3 cameraPosition, ref Vector3 cameraRotation)
        {
            _previousRT = device.GetRenderTargets();

            _reflectionViewMatrix = Matrix.Identity;
            _reflectionViewMatrix *= Matrix.CreateTranslation(new Vector3(-cameraPosition.X, cameraPosition.Y - 10f, -cameraPosition.Z));
            _reflectionViewMatrix *= Matrix.CreateRotationZ(cameraRotation.Z);
            _reflectionViewMatrix *= Matrix.CreateRotationY(cameraRotation.Y);
            _reflectionViewMatrix *= Matrix.CreateRotationX(-cameraRotation.X);

            var reflectionCamYCoord = -cameraPosition.Y + (Transform.Position.Y * 2.0f);
            _reflectionCameraPosition = new Vector3(cameraPosition.X, reflectionCamYCoord, cameraPosition.Z);

            device.SetRenderTarget(_reflectionRT);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
        }

        public void EndDraw(GraphicsDevice graphics)
        {
            graphics.SetRenderTargets(_previousRT);
        }
    }
}
