using C3DE.Components;
using C3DE.PostProcess;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE
{
    public class VRMobileRenderer : IRenderer
    {
        private RenderTarget2D _renderTargetLeft;
        private RenderTarget2D _renderTargetRight;
        private Rectangle _sideBySideLeftSpriteSize;
        private Rectangle _sideBySideRightSpriteSize;
        private SpriteBatch _spriteBatch;
        private PostProcessManager _postProcessManager;
        private bool _needsBufferUpdate;
        internal GUI _guiManager;

        public bool NeedsBufferUpdate
        {
            get { return _needsBufferUpdate; }
            set { _needsBufferUpdate = value; }
        }

        public VRMobileRenderer()
        {
            _postProcessManager = new PostProcessManager();
            _needsBufferUpdate = false;
        }

        public void LoadContent(ContentManager content)
        {
            Initialize();

            _guiManager = new GUI(_spriteBatch);
            _guiManager.LoadContent(content);
            _postProcessManager.LoadContent(content);
        }

        private void Initialize()
        {
            _spriteBatch = new SpriteBatch(Application.GraphicsDevice);

            // Left and right RenderTarget
            _renderTargetLeft = new RenderTarget2D(Application.GraphicsDevice, Screen.Width / 2, Screen.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _renderTargetRight = new RenderTarget2D(Application.GraphicsDevice, Screen.Width / 2, Screen.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);

            var aspect = (float)_renderTargetLeft.Width / (float)_renderTargetLeft.Height;

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
        }

        private void UpdateResolutionAndRenderTargets()
        {
#if ANDROID
            var viewportWidth = Screen.Width;
            var viewportHeight = Screen.Height;
#else
            var viewportWidth = Application.GraphicsDeviceManager.PreferredBackBufferWidth;
            var viewportHeight = Application.GraphicsDeviceManager.PreferredBackBufferHeight;
#endif
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
            // FIXME ...
            if (scene.Lights.Count == 0)
                return;

            //Application.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(Application.GraphicsDevice, camera);

            // Prepass, Update light, eye position, etc.
            for (int i = 0; i < scene.effects.Count; i++)
                scene.Materials[scene.materialsEffectIndex[i]].PrePass();

            // Pass, Update matrix, material attributes, etc.
            for (int i = 0; i < scene.RenderList.Count; i++)
            {
                if (scene.RenderList[i].Enabled)
                {
                    if (scene.RenderList[i].MaterialCount == 0)
                        scene.DefaultMaterial.Pass(scene.RenderList[i]);
                    else
                        scene.RenderList[i].Material.Pass(scene.RenderList[i]);

                    scene.RenderList[i].Draw(Application.GraphicsDevice);
                }
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
                    scene.Lights[i].ShadowGenerator.RenderShadows(Application.GraphicsDevice, scene.RenderList);

            renderObjects(scene, camera);
            renderUI(scene.Behaviours);
        }

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        public void render(Scene scene, Camera camera)
        {
            SetProjection(camera);

            Application.GraphicsDevice.SetRenderTarget(_renderTargetLeft);
            Application.GraphicsDevice.Clear(Color.Black);
            BaseDraw(scene, camera);

            Application.GraphicsDevice.SetRenderTarget(_renderTargetRight);
            Application.GraphicsDevice.Clear(Color.Black);
            BaseDraw(scene, camera);

            DrawRenderTargets(camera);
        }

        public void RenderEditor(Scene scene, Camera camera, RenderTarget2D target) { }

        private void DrawRenderTargets(Camera camera)
        {
            // Set RenderTargets
            Application.GraphicsDevice.SetRenderTarget(null);
            Application.GraphicsDevice.Clear(Color.Black);

            // Pass for left lens
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, null);
            _spriteBatch.Draw(_renderTargetLeft, _sideBySideLeftSpriteSize, Color.White);
            _spriteBatch.End();

            // Pass for right lens
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, null);
            _spriteBatch.Draw(_renderTargetRight, _sideBySideRightSpriteSize, Color.White);
            _spriteBatch.End();
        }
    }
}
