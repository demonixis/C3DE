using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public class DepthNormalRenderer : Renderer
    {
        internal RenderTarget2D m_DepthRT;
        internal RenderTarget2D m_NormalRT;
        private Effect m_Effect;

        public Camera Camera { get; set; }

        public Texture2D DepthBuffer => m_DepthRT;
        public Texture2D NormalBuffer => m_NormalRT;

        public override void Start()
        {
            base.Start();

            CreateRenderTargets();
            m_Effect = Application.Content.Load<Effect>("Shaders/DepthNormal");
            RenderQueue = -1;
        }

        public void CreateRenderTargets()
        {
            var device = Application.GraphicsDevice;
            var pp = device.PresentationParameters;
            m_DepthRT = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Single, DepthFormat.Depth24);
            m_NormalRT = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public override void ComputeBoundingInfos()
        {
        }

        public override void Draw(GraphicsDevice device)
        {
            if (Camera == null)
                Camera = Camera.Main;

            m_Effect.Parameters["View"].SetValue(Camera.m_ViewMatrix);
            m_Effect.Parameters["Projection"].SetValue(Camera.m_ProjectionMatrix);

            device.SetRenderTargets(m_NormalRT, m_DepthRT);
            device.Clear(Color.White);

            var renderList = Scene.current.renderList;

            for (int i = 0, l = renderList.Count; i < l; i++)
            {
                if (renderList[i] == this)
                    continue;

                m_Effect.Parameters["World"].SetValue(renderList[i].Transform.m_WorldMatrix);
                m_Effect.CurrentTechnique.Passes[0].Apply();
                renderList[i].Draw(device);
            }

            device.SetRenderTarget(null);
        }
    }
}

