using System;
using System.Collections.Generic;
using C3DE.Components;
using C3DE.PostProcess;
using Microsoft.Xna.Framework.Graphics;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace C3DE.Rendering
{
    public class OculusRiftRenderer : Renderer
    {
        const float PlayerSize = 1;
        OculusRift rift;
        RenderTarget2D[] renderTargetEye = new RenderTarget2D[2];
        Matrix playerMatrix = Matrix.CreateScale(PlayerSize);

        public OculusRiftRenderer()
            : base()
        {
            Application.Engine.IsFixedTimeStep = false;
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            var riftComponent = new OculusRiftComponent(Application.Engine);
            rift = riftComponent.rift;

            //int result = rift.Init(m_graphicsDevice);

            for (int eye = 0; eye < 2; eye++)
                renderTargetEye[eye] = rift.CreateRenderTargetForEye(eye);
        }

        public override void Render(Scene scene)
        {
            var camera = scene.cameras[0];

            // draw scene for both eyes into respective rendertarget
            for (int eye = 0; eye < 2; eye++)
            {
                m_graphicsDevice.SetRenderTarget(renderTargetEye[eye]);
                m_graphicsDevice.Clear(Color.Black);

                camera.projection = rift.GetProjectionMatrix(eye);
                camera.view = rift.GetEyeViewMatrix(eye, playerMatrix);

                RenderShadowMaps(scene);
                RenderObjects(scene, camera);

                //renderPostProcess(scene.postProcessPasses);
                RenderUI(scene.Behaviours);
            }

            // submit rendertargets to the Rift
            int result = rift.SubmitRenderTargets(renderTargetEye[0], renderTargetEye[1]);

            // show left eye view also on the monitor screen 
            DrawEyeViewIntoBackbuffer(0);
        }


        public override void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
        }

        protected override void renderPostProcess(List<PostProcessPass> passes)
        {
        }

        void DrawEyeViewIntoBackbuffer(int eye)
        {
            m_graphicsDevice.SetRenderTarget(null);
            m_graphicsDevice.Clear(Color.Black);

            var pp = m_graphicsDevice.PresentationParameters;

            int height = pp.BackBufferHeight;
            int width = Math.Min(pp.BackBufferWidth, (int)(height * rift.GetRenderTargetAspectRatio(eye)));
            int offset = (pp.BackBufferWidth - width) / 2;

            m_spriteBatch.Begin();
            m_spriteBatch.Draw(renderTargetEye[eye], new Rectangle(offset, 0, width, height), Color.White);
            m_spriteBatch.End();
        }
    }
}
