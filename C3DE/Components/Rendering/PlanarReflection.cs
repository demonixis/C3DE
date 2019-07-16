using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public class PlanarReflection : Behaviour
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

        public void BeginDraw(GraphicsDevice device, Camera camera)
        {
            _previousRT = device.GetRenderTargets();

            var position = camera._transform.Position;
            var rotation = camera._transform.Rotation;

            _reflectionViewMatrix = Matrix.Identity;
            _reflectionViewMatrix *= Matrix.CreateTranslation(new Vector3(-position.X, position.Y - 10f, -position.Z));
            _reflectionViewMatrix *= Matrix.CreateRotationZ(rotation.Z);
            _reflectionViewMatrix *= Matrix.CreateRotationY(rotation.Y);
            _reflectionViewMatrix *= Matrix.CreateRotationX(-rotation.X);

            var reflectionCamYCoord = -position.Y + (Transform.Position.Y * 2.0f);
            _reflectionCameraPosition = new Vector3(position.X, reflectionCamYCoord, position.Z);

            device.SetRenderTarget(_reflectionRT);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
        }

        public void EndDraw(GraphicsDevice graphics)
        {
            graphics.SetRenderTargets(_previousRT);
        }
    }
}
