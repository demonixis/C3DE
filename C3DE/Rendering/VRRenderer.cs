using C3DE.Components;
using C3DE.PostProcess;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Rendering
{
    public class VRRenderer : Renderer
    {
        private Effect _correctionFX;
        private RenderTarget2D[] _renderTargets;
        private Rectangle[] _sideBySideRects;
        private Matrix[] _projectionsMatrix;
        private Matrix[] _viewsMatrix;
        private Vector4 _hmdWarpParam;
        private Vector2 _scale;
        private Vector2 _scaleIn;
        private Vector2 _lensCenterLeft;
        private Vector2 _lensCenterRight;
        private bool _projectionUpdated;

        public bool DistortionCorrection
        {
            get; set;
        }

        public VRRenderer(bool enabledDistortionCorrection = false)
            : base()
        {
            DistortionCorrection = enabledDistortionCorrection;
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            _correctionFX = Application.Content.Load<Effect>("FX/PostProcess/OculusDistortionCorrection");

            // Left and right RenderTarget
            _renderTargets = new RenderTarget2D[]
            {
                new RenderTarget2D(m_graphicsDevice, OculusRiftDK2013_Metric.HResolution / 2, OculusRiftDK2013_Metric.VResolution, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents),
                new RenderTarget2D(m_graphicsDevice, OculusRiftDK2013_Metric.HResolution / 2, OculusRiftDK2013_Metric.VResolution, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents)
            };

            _sideBySideRects = new Rectangle[2];
            _projectionsMatrix = new Matrix[2];
            _viewsMatrix = new Matrix[2];

            var aspect = (float)_renderTargets[0].Width / (float)_renderTargets[0].Height;
            _scale = new Vector2(0.5f * (1.0f / OculusRiftDK2013_Metric.PostProcessScaleFactor), 0.5f * (1.0f / OculusRiftDK2013_Metric.PostProcessScaleFactor) * aspect);
            _scaleIn = new Vector2(2.0f, 2.0f / aspect);
            _lensCenterLeft = new Vector2(0.5f + OculusRiftDK2013_Metric.LensCenterOffset * 0.5f, 0.5f);
            _lensCenterRight = new Vector2(0.5f - OculusRiftDK2013_Metric.LensCenterOffset * 0.5f, 0.5f);
            _hmdWarpParam = new Vector4(OculusRiftDK2013_Metric.DistortionK[0], OculusRiftDK2013_Metric.DistortionK[1], OculusRiftDK2013_Metric.DistortionK[2], OculusRiftDK2013_Metric.DistortionK[3]);

            UpdateResolutionAndRenderTargets();
        }

        public void SetProjection(Camera camera)
        {
            var aspectRatio = (OculusRiftDK2013_Metric.HScreenSize * 0.5f) / OculusRiftDK2013_Metric.VScreenSize;
            var fov_d = OculusRiftDK2013_Metric.EyeToScreenDistance;
            var fov_x = OculusRiftDK2013_Metric.VScreenSize * 0.71f;
            var yfov = 2.0f * (float)Math.Atan(fov_x / fov_d);

            camera.Aspect = aspectRatio;
            camera.FieldOfView = yfov;
            _projectionUpdated = true;
        }

        private void ComputeViewProjMatrix(ref Matrix projection, ref Matrix view)
        {
            var viewCenter = OculusRiftDK2013_Metric.HScreenSize * 0.212f; // 0.25f
            var eyeProjectionShift = viewCenter - OculusRiftDK2013_Metric.LensSeparationDistance * 0.5f;
            var projectionCenterOffset = 4.0f * eyeProjectionShift / OculusRiftDK2013_Metric.HScreenSize;

            var projCenter = projection;
            _projectionsMatrix[0] = Matrix.CreateTranslation(projectionCenterOffset, 0, 0) * projCenter;
            _projectionsMatrix[1] = Matrix.CreateTranslation(-projectionCenterOffset, 0, 0) * projCenter;

            var halfIPD = OculusRiftDK2013_Metric.InterpupillaryDistance * 0.5f;
            _viewsMatrix[0] = Matrix.CreateTranslation(halfIPD, 0, 0) * view;
            _viewsMatrix[1] = Matrix.CreateTranslation(-halfIPD, 0, 0) * view;
        }

        private void UpdateResolutionAndRenderTargets()
        {
            var viewportWidth = Screen.Width;
            var viewportHeight = Screen.Height;

            _sideBySideRects[0] = new Rectangle(0, 0, viewportWidth / 2, viewportHeight);
            _sideBySideRects[1] = new Rectangle(viewportWidth / 2, 0, viewportWidth / 2, viewportHeight);

            Screen.Setup(_sideBySideRects[0].Width, _sideBySideRects[0].Height, null, null);
        }

        protected override void renderPostProcess(List<PostProcessPass> passes) { }
        public override void RenderEditor(Scene scene, Camera camera, RenderTarget2D target) { }

        private void BaseDraw(Scene scene, Camera camera)
        {
            RenderShadowMaps(scene);
            RenderObjects(scene, camera);
            RenderUI(scene.Behaviours);
        }

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        public override void Render(Scene scene)
        {
            var camera = scene.cameras[0];

            if (!_projectionUpdated)
                SetProjection(scene.cameras[0]);

            ComputeViewProjMatrix(ref camera.projection, ref camera.view);

            for (int i = 0; i < 2; i++)
            {
                m_graphicsDevice.SetRenderTarget(_renderTargets[i]);
                m_graphicsDevice.Clear(Color.Black);
                camera.projection = _projectionsMatrix[i];
                camera.view = _viewsMatrix[i];
                BaseDraw(scene, camera);
            }

            if (DistortionCorrection)
                DrawRenderTargetsWithCorrection(camera);
            else
                DrawRenderTargets(camera);
        }

        private void DrawRenderTargets(Camera camera)
        {
            // Set RenderTargets
            m_graphicsDevice.SetRenderTarget(null);
            m_graphicsDevice.Clear(Color.Black);

            for (int i = 0; i < 2; i++)
            {
                m_spriteBatch.Begin(i == 0 ? SpriteSortMode.Immediate : SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, null);
                m_spriteBatch.Draw(_renderTargets[i], _sideBySideRects[i], Color.White);
                m_spriteBatch.End();
            }
        }

        private void DrawRenderTargetsWithCorrection(Camera camera)
        {
            // Set RenderTargets
            m_graphicsDevice.SetRenderTarget(null);
            m_graphicsDevice.Clear(Color.Black);

            _correctionFX.Parameters["HmdWarpParam"].SetValue(_hmdWarpParam);
            _correctionFX.Parameters["Scale"].SetValue(_scale);
            _correctionFX.Parameters["ScaleIn"].SetValue(_scaleIn);

            // Pass for left lens
            _correctionFX.Parameters["LensCenter"].SetValue(_lensCenterLeft);
            m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, _correctionFX);
            m_spriteBatch.Draw(_renderTargets[0], _sideBySideRects[0], Color.White);
            m_spriteBatch.End();

            // Pass for right lens
            _correctionFX.Parameters["LensCenter"].SetValue(_lensCenterRight);
            m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, _correctionFX);
            m_spriteBatch.Draw(_renderTargets[1], _sideBySideRects[1], Color.White);
            m_spriteBatch.End();
        }
    }
}