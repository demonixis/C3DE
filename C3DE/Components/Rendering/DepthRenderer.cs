using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public class DepthRenderer
    {
        internal RenderTarget2D m_DepthRT;
        private Effect m_Effect;

        public Camera Camera { get; set; }
        public Texture2D DepthBuffer => m_DepthRT;
        public bool Enabled { get; set; }
        public bool ExcludeSkybox { get; set; } = true;

        public DepthRenderer()
        {
            CreateRenderTargets();
            m_Effect = Application.Content.Load<Effect>("Shaders/Depth");
        }

        public void CreateRenderTargets()
        {
            var device = Application.GraphicsDevice;
            var pp = device.PresentationParameters;
            m_DepthRT = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
        }

        public void Draw(GraphicsDevice device)
        {
            using (device.GeometryState())
            {
                if (Camera == null)
                    Camera = Camera.Main;

                m_Effect.Parameters["View"].SetValue(Camera.m_ViewMatrix);
                m_Effect.Parameters["Projection"].SetValue(Camera.m_ProjectionMatrix);

                var previousRTs = device.GetRenderTargets();

                device.SetRenderTarget(m_DepthRT);
                device.Clear(Color.White);

                var scene = Scene.current;
                var skybox = scene.RenderSettings.skybox;

                if (!ExcludeSkybox && skybox.Enabled)
                {
                    m_Effect.Parameters["World"].SetValue(skybox.WorldMatrix);
                    m_Effect.CurrentTechnique.Passes[0].Apply();
                    skybox.DrawNoEffect(device);
                }

                var renderList = scene.renderList;

                for (int i = 0, l = renderList.Count; i < l; i++)
                {
                    m_Effect.Parameters["World"].SetValue(renderList[i].Transform.m_WorldMatrix);
                    m_Effect.CurrentTechnique.Passes[0].Apply();
                    renderList[i].Draw(device);
                }

                device.SetRenderTargets(previousRTs);
            }
        }
    }
}

