using System;
using C3DE.Components;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.PostProcessing
{
    public class GlobalFog : PostProcessPass
    {
        private Effect m_Effect;
        private RenderTarget2D m_SceneRenderTarget;
        private RenderTarget2D m_DepthBuffer;
        private QuadRenderer m_QuadRenderer;
        private RenderSettings m_RenderSettings;

        public bool DistanceFog = true;
        public bool ExcludeFarPixels = true;
        public bool UseRadialDistance = false;
        public bool HeightFog = true;
        public float Height = 50;
        public float HeightDensity = 0.5f;

        public bool ExcludeSkybox
        {
            get
            {
                var renderer = Application.Engine.Renderer;
                var forward = renderer as ForwardRenderer;
                if (forward != null)
                    return forward.DepthRenderer.ExcludeSkybox;

                return true;
            }
            set
            {
                var renderer = Application.Engine.Renderer;
                var forward = renderer as ForwardRenderer;
                if (forward != null)
                    forward.DepthRenderer.ExcludeSkybox = value;
            }
        }

        public GlobalFog(GraphicsDevice graphics, bool excludeSkybox = false) 
            : base(graphics)
        {
            ExcludeSkybox = excludeSkybox;
        }

        public override void Initialize(ContentManager content)
        {
            m_Effect = content.Load<Effect>("Shaders/PostProcessing/GlobalFog");
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
            m_RenderSettings = Scene.current.RenderSettings;
            m_RenderSettings.FogEnabled = false;
        }

        public override void Draw(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            if (m_RenderSettings.FogMode == FogMode.None)
                return;

            m_GraphicsDevice.SetRenderTarget(m_SceneRenderTarget);
            m_GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            var camera = Camera.Main;
            var cameraTransform = camera.m_Transform;
            var frustumCorners = camera.CalculateFrustumCorners(new Rectangle(0, 0, 1, 1), camera.Far, 0);
            var bottomLeft = cameraTransform.TransformVector(frustumCorners[4]);
            var topLeft = cameraTransform.TransformVector(frustumCorners[5]);
            var topRight = cameraTransform.TransformVector(frustumCorners[6]);
            var bottomRight = cameraTransform.TransformVector(frustumCorners[7]);
            var frustumCornersArray = MatrixExtensions.CreateFromVector3(bottomLeft, bottomRight, topLeft, topRight);
            var camPos = cameraTransform.Position;
            var FdotC = camPos.Y - Height;
            var paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
            var excludeDepth = (ExcludeFarPixels ? 1.0f : 2.0f);
            var sceneMode = m_RenderSettings.FogMode;
            var sceneDensity = m_RenderSettings.FogDensity;
            var heightParams = new Vector4(Height, FdotC, paramK, HeightDensity * 0.5f);
            var distanceParams = new Vector4(-Math.Max(m_RenderSettings.FogStart, 0.0f), excludeDepth, 0, 0);
            var linear = sceneMode == FogMode.Linear;
            var diff = linear ? m_RenderSettings.FogEnd - m_RenderSettings.FogStart : 0.0f;
            var invDiff = Math.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
            var sceneParams = new Vector4(sceneDensity * 1.2011224087f, sceneDensity * 1.4426950408f, linear ? -invDiff : 0.0f, linear ? m_RenderSettings.FogEnd * invDiff : 0.0f);
            var fogMode = new Vector4((int)sceneMode, UseRadialDistance ? 1 : 0, 0, 0);
            var textureSamplerTexelSize = new Vector4(1.0f / (float)renderTarget.Width, 1.0f / (float)renderTarget.Height, renderTarget.Width, renderTarget.Height);
            var projectionParams = new Vector4(1.0f, Camera.Main.Near, Camera.Main.Far, 1.0f / Camera.Main.Far);

            m_Effect.Parameters["FrustumCornersWS"].SetValue(frustumCornersArray);
            m_Effect.Parameters["CameraWS"].SetValue(camPos);
            m_Effect.Parameters["HeightParams"].SetValue(heightParams);
            m_Effect.Parameters["DistanceParams"].SetValue(distanceParams);
            m_Effect.Parameters["FogParams"].SetValue(sceneParams);
            m_Effect.Parameters["FogMode"].SetValue(fogMode);
            m_Effect.Parameters["FogColor"].SetValue(Scene.current.RenderSettings.FogColor.ToVector4());
            m_Effect.Parameters["TextureSamplerTexelSize"].SetValue(textureSamplerTexelSize);
            m_Effect.Parameters["ProjectionParams"].SetValue(projectionParams);
            m_Effect.Parameters["TargetTexture"].SetValue(renderTarget);
            m_Effect.Parameters["DepthTexture"].SetValue(m_DepthBuffer);

            var passIndex = 0;
            if (DistanceFog && HeightFog)
                passIndex = 0;
            else if (DistanceFog)
                passIndex = 1;
            else
                passIndex = 2;

            m_Effect.CurrentTechnique.Passes[passIndex].Apply();
            m_QuadRenderer.RenderFullscreenQuad(m_GraphicsDevice);

            m_GraphicsDevice.SetRenderTarget(null);
            m_GraphicsDevice.Textures[1] = m_SceneRenderTarget;

            var viewport = m_GraphicsDevice.Viewport;
            m_GraphicsDevice.SetRenderTarget(renderTarget);

            DrawFullscreenQuad(spriteBatch, m_SceneRenderTarget, viewport.Width, viewport.Height, null);
        }
    }
}
