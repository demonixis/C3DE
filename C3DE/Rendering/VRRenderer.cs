using System;
using System.Collections.Generic;
using C3DE.Components;
using C3DE.PostProcess;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using C3DE.VR;

namespace C3DE.Rendering
{
    public class VRRenderer : Renderer
    {
        private IVRDevice vrDevice;
        private RenderTarget2D[] renderTargetEye = new RenderTarget2D[2];
        private Matrix playerMatrix = Matrix.CreateScale(1);

        public VRRenderer(GraphicsDevice graphics, IVRDevice device)
            : base(graphics)
        {
            Application.Engine.IsFixedTimeStep = false;
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            for (int eye = 0; eye < 2; eye++)
                renderTargetEye[eye] = vrDevice.CreateRenderTargetForEye(eye);
        }

        public override void Render(Scene scene)
        {
            var camera = scene.cameras[0];

            for (int eye = 0; eye < 2; eye++)
            {
                m_graphicsDevice.SetRenderTarget(renderTargetEye[eye]);
                m_graphicsDevice.Clear(Color.Black);

                camera.projection = vrDevice.GetProjectionMatrix(eye);
                camera.view = vrDevice.GetViewMatrix(eye, playerMatrix);

                RenderShadowMaps(scene);
                RenderObjects(scene, camera);
                //renderPostProcess(scene.postProcessPasses);
                //RenderUI(scene.Behaviours);
            }
  
            vrDevice.SubmitRenderTargets(renderTargetEye[0], renderTargetEye[1]);

            // show left eye view also on the monitor screen 
            DrawEyeViewIntoBackbuffer(0);
        }

        public override void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
        }

        protected override void renderPostProcess(List<PostProcessPass> passes)
        {
        }

        private void DrawEyeViewIntoBackbuffer(int eye)
        {
            m_graphicsDevice.SetRenderTarget(null);
            m_graphicsDevice.Clear(Color.Black);

            var pp = m_graphicsDevice.PresentationParameters;

            int height = pp.BackBufferHeight;
            int width = Math.Min(pp.BackBufferWidth, (int)(height * vrDevice.GetRenderTargetAspectRatio(eye)));
            int offset = (pp.BackBufferWidth - width) / 2;

            m_spriteBatch.Begin();
            m_spriteBatch.Draw(renderTargetEye[eye], new Rectangle(offset, 0, width, height), Color.White);
            m_spriteBatch.End();
        }
    }
}
