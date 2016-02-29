using C3DE.Components;
using C3DE.UI;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Rendering
{
    public class VRRenderer : IRenderer
    {
        private Effect oculusRiftDistortionShader;
        private RenderTarget2D _renderTargetLeft;
        private RenderTarget2D _renderTargetRight;
        private Rectangle _sideBySideLeftSpriteSize;
        private Rectangle _sideBySideRightSpriteSize;
        private Matrix _projectionMatrixLeft;
        private Matrix _projectionMatrixRight;
        private Matrix _viewMatrixLeft;
        private Matrix _viewMatrixRight;
        private Vector4 _hmdWarpParam;
        private Vector2 _scale;
        private Vector2 _scaleIn;
        private Vector2 _lensCenterLeft;
        private Vector2 _lensCenterRight;
        private bool _projectionUpdated;

        private GraphicsDevice _device;
        private SpriteBatch _spriteBatch;
        private bool _needsBufferUpdate;
        internal GUI _guiManager;

        public bool NeedsBufferUpdate
        {
            get { return _needsBufferUpdate; }
            set { _needsBufferUpdate = value; }
        }

        public bool DistortionCorrection
        {
            get; set;
        }

        public VRRenderer()
        {
            _needsBufferUpdate = false;
        }

        public void Initialize(ContentManager content)
        {
            Initialize();

            _guiManager = new GUI(_spriteBatch);
            _guiManager.LoadContent(content);
        }

        private void Initialize()
        {
            _device = Application.GraphicsDevice;
            _spriteBatch = new SpriteBatch(_device);

            oculusRiftDistortionShader = Application.Content.Load<Effect>("FX/PostProcess/OculusDistortionCorrection");

            // Left and right RenderTarget
            _renderTargetLeft = new RenderTarget2D(_device, OculusRiftDK2013_Metric.HResolution / 2, OculusRiftDK2013_Metric.VResolution, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _renderTargetRight = new RenderTarget2D(_device, OculusRiftDK2013_Metric.HResolution / 2, OculusRiftDK2013_Metric.VResolution, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            var aspect = (float)_renderTargetLeft.Width / (float)_renderTargetLeft.Height;
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
            _projectionMatrixLeft = Matrix.CreateTranslation(projectionCenterOffset, 0, 0) * projCenter;
            _projectionMatrixRight = Matrix.CreateTranslation(-projectionCenterOffset, 0, 0) * projCenter;

            var halfIPD = OculusRiftDK2013_Metric.InterpupillaryDistance * 0.5f;
            _viewMatrixLeft = Matrix.CreateTranslation(halfIPD, 0, 0) * view;
            _viewMatrixRight = Matrix.CreateTranslation(-halfIPD, 0, 0) * view;
        }

        private void UpdateResolutionAndRenderTargets()
        {
			var viewportWidth = Screen.Width;
            var viewportHeight = Screen.Height;

            _sideBySideLeftSpriteSize = new Rectangle(0, 0, viewportWidth / 2, viewportHeight);
            _sideBySideRightSpriteSize = new Rectangle(viewportWidth / 2, 0, viewportWidth / 2, viewportHeight);

            Screen.Setup(_sideBySideLeftSpriteSize.Width, _sideBySideLeftSpriteSize.Height, null, null);
        }

        /// <summary>
        /// Render renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        private void renderObjects(Scene scene, Camera camera)
        {
            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(_device, camera);

            // Prepass, Update light, eye position, etc.
            for (int i = 0; i < scene.effects.Count; i++)
                scene.materials[scene.materialsEffectIndex[i]].PrePass(camera);

            // Pass, Update matrix, material attributes, etc.
            // Note: The renderList contains ONLY enabled components/objects.
            for (int i = 0; i < scene.renderList.Count; i++)
            {
                scene.renderList[i].Material.Pass(scene.RenderList[i]);
                scene.renderList[i].Draw(_device);
            }
        }

        private void renderUI(List<Behaviour> scripts)
        {
            var size = scripts.Count;

            if (size > 0)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

                for (int i = 0; i < size; i++)
                    if (scripts[i].Enabled)
                        scripts[i].OnGUI(_guiManager);

                _spriteBatch.End();
            }
        }

        private void renderPostProcess() { }

        private void BaseDraw(Scene scene, Camera camera)
        {
            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].ShadowGenerator.Enabled)
                    scene.Lights[i].ShadowGenerator.RenderShadows(_device, scene.RenderList);

            renderObjects(scene, camera);
            renderUI(scene.Behaviours);
        }

        public void RenderEditor(Scene scene, Camera camera, RenderTarget2D target) { }

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        public void Render(Scene scene)
        {
            var camera = scene.cameras[0];

            if (!_projectionUpdated)
                SetProjection(scene.cameras[0]);

            ComputeViewProjMatrix(ref camera.projection, ref camera.view);

            _device.SetRenderTarget(_renderTargetLeft);
            _device.Clear(Color.Black);
            camera.projection = _projectionMatrixLeft;
            camera.view = _viewMatrixLeft;
            BaseDraw(scene, camera);

            _device.SetRenderTarget(_renderTargetRight);
            _device.Clear(Color.Black);
            camera.projection = _projectionMatrixRight;
            camera.view = _viewMatrixRight;
            BaseDraw(scene, camera);

            if (DistortionCorrection)
                DrawRenderTargetsWithCorrection(camera);
            else
                DrawRenderTargets(camera);
        }

        private void DrawRenderTargets(Camera camera)
        {
            // Set RenderTargets
            _device.SetRenderTarget(null);
            _device.Clear(Color.Black);

            // Pass for left lens
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, null);
            _spriteBatch.Draw(_renderTargetLeft, _sideBySideLeftSpriteSize, Color.White);
            _spriteBatch.End();

            // Pass for right lens
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, null);
            _spriteBatch.Draw(_renderTargetRight, _sideBySideRightSpriteSize, Color.White);
            _spriteBatch.End();
        }

        private void DrawRenderTargetsWithCorrection(Camera camera)
        {
            // Set RenderTargets
            _device.SetRenderTarget(null);
            _device.Clear(Color.Black);

            oculusRiftDistortionShader.Parameters["HmdWarpParam"].SetValue(_hmdWarpParam);
            oculusRiftDistortionShader.Parameters["Scale"].SetValue(_scale);
            oculusRiftDistortionShader.Parameters["ScaleIn"].SetValue(_scaleIn);

            // Pass for left lens
            oculusRiftDistortionShader.Parameters["LensCenter"].SetValue(_lensCenterLeft);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, oculusRiftDistortionShader);
            _spriteBatch.Draw(_renderTargetLeft, _sideBySideLeftSpriteSize, Color.White);
            _spriteBatch.End();

            // Pass for right lens
            oculusRiftDistortionShader.Parameters["LensCenter"].SetValue(_lensCenterRight);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, oculusRiftDistortionShader);
            _spriteBatch.Draw(_renderTargetRight, _sideBySideRightSpriteSize, Color.White);
            _spriteBatch.End();
        }
    }
}