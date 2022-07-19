using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering.Passes
{
    public class DepthPass : RenderPass
    {
        public DepthPass(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            var pp = _graphicsDevice.PresentationParameters;
            _renderTarget = new RenderTarget2D(_graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
            _effect = content.Load<Effect>("Shaders/Depth");
        }

        public override void Render(Scene scene, Camera camera)
        {
            var previousRTs = _graphicsDevice.GetRenderTargets();
            var cameraViewMatrix = camera._viewMatrix;
            var cameraProjectionMatrix = camera._projectionMatrix;
            var renderList = scene._renderList;

            _graphicsDevice.SetRenderTarget(_renderTarget);
            _graphicsDevice.Clear(Color.Transparent);

            _effect.Parameters["View"].SetValue(cameraViewMatrix);
            _effect.Parameters["Projection"].SetValue(cameraProjectionMatrix);

            for (int i = 0, l = renderList.Count; i < l; i++)
            {
                if (renderList[i] is LensFlare) continue;

                _effect.Parameters["World"].SetValue(renderList[i].Transform._worldMatrix);
                _effect.CurrentTechnique.Passes[0].Apply();
                renderList[i].Draw(_graphicsDevice);
            }

            _graphicsDevice.SetRenderTargets(previousRTs);
        }
    }
}
