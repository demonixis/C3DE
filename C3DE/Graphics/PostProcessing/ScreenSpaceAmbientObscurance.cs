using C3DE.Components;
using C3DE.Graphics.Rendering;
using C3DE.Utils;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class ScreenSpaceAmbientObscurance : PostProcessPass
    {
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;
        private RenderTarget2D m_DepthBuffer;
        private QuadRenderer m_QuadRenderer;

        public float Intensity = 0.5f;
        public float Radius = 0.2f;
        public int blurIterations = 1;
        public float BlurFilterDistance = 1.25f;
        public int downsample = 0;
        public Texture2D RandomTexture = null;

        public ScreenSpaceAmbientObscurance(GraphicsDevice graphics) : base(graphics)
        {
        }

        protected override void OnVRChanged(VRService service)
        {
            base.OnVRChanged(service);
            m_SceneRenderTarget.Dispose();
            m_SceneRenderTarget = GetRenderTarget();
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/SSAmbientObscurance");
            m_SceneRenderTarget = GetRenderTarget();

            var renderer = Application.Engine.Renderer;
            if (renderer is ForwardRenderer)
            {
                var forward = (ForwardRenderer)renderer;
                forward.DepthRenderer.Enabled = true;
                m_DepthBuffer = forward.DepthRenderer.m_DepthRT;
            }
            else if (renderer is DeferredRenderer)
            {
                var forward = (DeferredRenderer)renderer;
                m_DepthBuffer = forward.DepthBuffer;
            }
            else if (renderer is LightPrePassRenderer)
            {
                var lpp = (LightPrePassRenderer)renderer;
                m_DepthBuffer = lpp.DepthBuffer;
            }

            m_QuadRenderer = new QuadRenderer(m_GraphicsDevice);

            if (RandomTexture == null)
                RandomTexture = GraphicsHelper.CreateRandomTexture(128);
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRT)
        {
            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            var camera = Camera.Main;
            var cameraProjection = camera.m_ProjectionMatrix;
            var invertProjection = Matrix.Invert(cameraProjection);
            var projInfo = new Vector4
                ((-2.0f / cameraProjection[0, 0]),
                 (-2.0f / cameraProjection[1, 1]),
                 ((1.0f - cameraProjection[0, 2]) / cameraProjection[0, 0]),
                 ((1.0f + cameraProjection[1, 2]) / cameraProjection[1, 1]));

            m_Effect.Parameters["ProjInfo"].SetValue(projInfo);
            m_Effect.Parameters["ProjectionInv"].SetValue(invertProjection);
            //m_Effect.Parameters["RandTexture"].SetValue(RandomTexture);
            m_Effect.Parameters["Radius"].SetValue(Radius);
            m_Effect.Parameters["Radius2"].SetValue(Radius * Radius);
            m_Effect.Parameters["Intensity"].SetValue(Intensity);
            m_Effect.Parameters["BlurFilterDistance"].SetValue(BlurFilterDistance);
            m_Effect.Parameters["NearClip"].SetValue(camera.Near);
            m_Effect.Parameters["FarClip"].SetValue(camera.Far);
            m_Effect.Parameters["DepthTexture"].SetValue(m_DepthBuffer);

            var rtW = sceneRT.Width;
            var rtH = sceneRT.Height;

            RenderTarget2D tmpRt = RenderTexture.GetTemporary(rtW >> downsample, rtH >> downsample);
            RenderTarget2D tmpRt2;

            Blit(sceneRT, tmpRt, 0);

            if (downsample > 0)
            {
                tmpRt2 = RenderTexture.GetTemporary(rtW, rtH);
                Blit(tmpRt, tmpRt2, 4);

                RenderTexture.ReleaseTemporary(tmpRt);
                tmpRt = tmpRt2;
            }

            for (int i = 0; i < blurIterations; i++)
            {
                m_Effect.Parameters["Axis"].SetValue(new Vector2(1.0f, 0.0f));
                tmpRt2 = RenderTexture.GetTemporary(rtW, rtH);
                Blit(tmpRt, tmpRt2, 1);
                RenderTexture.ReleaseTemporary(tmpRt);

                m_Effect.Parameters["Axis"].SetValue(new Vector2(0.0f, 1.0f));
                tmpRt = RenderTexture.GetTemporary(rtW, rtH);
                Blit(tmpRt2, tmpRt, 1);
                RenderTexture.ReleaseTemporary(tmpRt2);
            }

            m_Effect.Parameters["AOTexture"].SetValue(tmpRt);
            Blit(sceneRT, m_SceneRenderTarget, 2);

            RenderTexture.ReleaseAll();

            DrawFullscreenQuad(spriteBatch, sceneRT, m_SceneRenderTarget, m_Effect);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            var viewport = m_GraphicsDevice.Viewport;
            m_GraphicsDevice.SetRenderTarget(sceneRT);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, m_SceneRenderTarget.Width, m_SceneRenderTarget.Height, null);
        }

        private void Blit(RenderTarget2D source, RenderTarget2D dest, int pass)
        {
            var textureSamplerTexelSize = new Vector4(1.0f / (float)source.Width, 1.0f / (float)source.Height, source.Width, source.Height);

            m_GraphicsDevice.SetRenderTarget(dest);
            m_Effect.Parameters["MainTexture"].SetValue(source);
            m_Effect.Parameters["MainTextureTexelSize"].SetValue(textureSamplerTexelSize);
            m_Effect.CurrentTechnique.Passes[pass].Apply();
            m_QuadRenderer.RenderFullscreenQuad(m_GraphicsDevice);
        }
    }
}
