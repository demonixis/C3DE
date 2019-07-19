using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public class DepthRenderer
    {
        internal RenderTarget2D _depthRT;
        private Effect _effect;

        public Camera Camera { get; set; }
        public Texture2D DepthBuffer => _depthRT;
        public bool Enabled { get; set; }
        public bool ExcludeSkybox { get; set; } = true;

        public DepthRenderer()
        {
            CreateRenderTargets();
            _effect = Application.Content.Load<Effect>("Shaders/Depth");
        }

        public void CreateRenderTargets()
        {
            var device = Application.GraphicsDevice;
            var pp = device.PresentationParameters;
            _depthRT = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
        }

        public void Draw(GraphicsDevice device)
        {
            using (device.GeometryState())
            {
                if (Camera == null)
                    Camera = Camera.Main;

                _effect.Parameters["View"].SetValue(Camera._viewMatrix);
                _effect.Parameters["Projection"].SetValue(Camera._projectionMatrix);

                var previousRTs = device.GetRenderTargets();

                device.SetRenderTarget(_depthRT);
                device.Clear(Color.White);

                var scene = Scene.current;
                var skybox = scene.RenderSettings.skybox;

                if (!ExcludeSkybox && skybox.Enabled)
                {
                    _effect.Parameters["World"].SetValue(skybox.WorldMatrix);
                    _effect.CurrentTechnique.Passes[0].Apply();
                    skybox.DrawNoEffect(device);
                }

                var renderList = scene._renderList;

                for (int i = 0, l = renderList.Count; i < l; i++)
                {
                    _effect.Parameters["World"].SetValue(renderList[i].Transform._worldMatrix);
                    _effect.CurrentTechnique.Passes[0].Apply();
                    renderList[i].Draw(device);
                }

                device.SetRenderTargets(previousRTs);
            }
        }
    }
}

