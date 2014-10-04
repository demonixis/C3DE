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
    public sealed class OculusRiftDK2013_Metric
    {
        public const int HResolution = 1280;
        public const int VResolution = 800;
        public const float HScreenSize = 0.149759993f;
        public const float VScreenSize = 0.0935999975f;
        public const float VScreenCenter = 0.0467999987f;
        public const float EyeToScreenDistance = 0.0410000011f;
        public const float LensSeparationDistance = 0.0635000020f;
        public const float InterpupillaryDistance = 0.0640000030f;
        public static readonly float[] DistortionK = new float[4] { 1.0f, 0.219999999f, 0.239999995f, 0.0f };
        public static readonly float[] ChromaAbCorrection = new float[4] { 0.995999992f, -0.00400000019f, 1.01400006f, 0.0f };
        public const float PostProcessScaleFactor = 1.714605507808412f;
        public const float LensCenterOffset = 0.151976421f;
    }

    public class VRRenderer : IRenderer
    {
        private Effect oculusRiftDistortionShader;
        private RenderTarget2D renderTargetLeft;
        private RenderTarget2D renderTargetRight;
        private Texture2D renderTextureLeft;
        private Texture2D renderTextureRight;
        private float scaleImageFactor;
        private int viewportWidth;
        private int viewportHeight;
        private Rectangle sideBySideLeftSpriteSize;
        private Rectangle sideBySideRightSpriteSize;
        private Matrix projLeft;
        private Matrix projRight;
        private Matrix viewLeft;
        private Matrix viewRight;

        private SpriteBatch _spriteBatch;
        private PostProcessManager _postProcessManager;
        private bool _needsBufferUpdate;
        internal GUI _guiManager;

        public bool NeedsBufferUpdate
        {
            get { return _needsBufferUpdate; }
            set { _needsBufferUpdate = value; }
        }

        public VRRenderer()
        {
            _postProcessManager = new PostProcessManager();
            _needsBufferUpdate = false;

            scaleImageFactor = 0.71f;
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

            Application.GraphicsDeviceManager.PreferredBackBufferWidth = (int)Math.Ceiling(OculusRiftDK2013_Metric.HResolution * scaleImageFactor);
            Application.GraphicsDeviceManager.PreferredBackBufferHeight = (int)Math.Ceiling(OculusRiftDK2013_Metric.VResolution * scaleImageFactor);
            // Application.GraphicsDeviceManager.IsFullScreen = true;
            Application.GraphicsDeviceManager.ApplyChanges();

            oculusRiftDistortionShader = Application.Content.Load<Effect>("FX/PostProcess/OculusRift");

            // Init left and right RenderTarget
            renderTargetLeft = new RenderTarget2D(Application.GraphicsDevice, Application.GraphicsDeviceManager.PreferredBackBufferWidth / 2, Application.GraphicsDeviceManager.PreferredBackBufferHeight);
            renderTargetRight = new RenderTarget2D(Application.GraphicsDevice, Application.GraphicsDeviceManager.PreferredBackBufferWidth / 2, Application.GraphicsDeviceManager.PreferredBackBufferHeight);

            UpdateResolutionAndRenderTargets();
        }

        public void InitializeCamera(Camera camera)
        {
            var aspectRatio = (OculusRiftDK2013_Metric.HScreenSize * 0.5f) / OculusRiftDK2013_Metric.VScreenSize;
            var fov_d = OculusRiftDK2013_Metric.EyeToScreenDistance;
            var fov_x = OculusRiftDK2013_Metric.VScreenSize * scaleImageFactor;
            var yfov = 2.0f * (float)Math.Atan(fov_x / fov_d);

            // Set ProjectionMatrix
            camera.projection = Matrix.CreatePerspectiveFieldOfView(yfov, aspectRatio, 1.0f, 100000.0f);
            _init = true;
        }

        private bool _init;

        private void SetProjectionOffset(ref Matrix projection, ref Matrix view)
        {
            var viewCenter = OculusRiftDK2013_Metric.HScreenSize * 0.212f; // 0.25f
            var eyeProjectionShift = viewCenter - OculusRiftDK2013_Metric.LensSeparationDistance * 0.5f;
            var projectionCenterOffset = 4.0f * eyeProjectionShift / OculusRiftDK2013_Metric.HScreenSize;

            var projCenter = projection;
            projLeft = Matrix.CreateTranslation(projectionCenterOffset, 0, 0) * projCenter;
            projRight = Matrix.CreateTranslation(-projectionCenterOffset, 0, 0) * projCenter;

            var halfIPD = OculusRiftDK2013_Metric.InterpupillaryDistance * 0.5f;
            viewLeft = Matrix.CreateTranslation(halfIPD, 0, 0) * view;
            viewRight = Matrix.CreateTranslation(-halfIPD, 0, 0) * view;
        }

        private void UpdateResolutionAndRenderTargets()
        {
            if (viewportWidth != Application.GraphicsDevice.Viewport.Width || viewportHeight != Application.GraphicsDevice.Viewport.Height)
            {
                viewportWidth = Application.GraphicsDevice.Viewport.Width;
                viewportHeight = Application.GraphicsDevice.Viewport.Height;
                sideBySideLeftSpriteSize = new Rectangle(0, 0, viewportWidth / 2, viewportHeight);
                sideBySideRightSpriteSize = new Rectangle(viewportWidth / 2, 0, viewportWidth / 2, viewportHeight);
            }

            Screen.Setup(sideBySideLeftSpriteSize.Width, sideBySideLeftSpriteSize.Height, null, null);
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

            Application.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

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

                Application.GraphicsDevice.BlendState = BlendState.Opaque;
                Application.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                Application.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
        }

        private void renderPostProcess()
        {

        }

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
            if (!_init)
                InitializeCamera(camera);

            SetProjectionOffset(ref camera.projection, ref camera.view);

            Application.GraphicsDevice.SetRenderTarget(renderTargetLeft);
            Application.GraphicsDevice.Clear(Color.Black);
            camera.projection = projLeft;
            camera.view = viewLeft;
            BaseDraw(scene, camera);

            Application.GraphicsDevice.SetRenderTarget(renderTargetRight);
            Application.GraphicsDevice.Clear(Color.Black);
            camera.projection = projRight;
            camera.view = viewRight;
            BaseDraw(scene, camera);

            DrawOculusRenderTargets();
        }

        public void RenderEditor(Scene scene, Camera camera, RenderTarget2D target) { }

        private void DrawOculusRenderTargets()
        {
            // Set RenderTargets
            Application.GraphicsDevice.SetRenderTarget(null);
            renderTextureLeft = (Texture2D)renderTargetLeft;
            renderTextureRight = (Texture2D)renderTargetRight;
            Application.GraphicsDevice.Clear(Color.Black);

            //Set the four Distortion params of the oculus
            oculusRiftDistortionShader.Parameters["distK0"].SetValue(OculusRiftDK2013_Metric.DistortionK[0]);
            oculusRiftDistortionShader.Parameters["distK1"].SetValue(OculusRiftDK2013_Metric.DistortionK[1]);
            oculusRiftDistortionShader.Parameters["distK2"].SetValue(OculusRiftDK2013_Metric.DistortionK[2]);
            oculusRiftDistortionShader.Parameters["distK3"].SetValue(OculusRiftDK2013_Metric.DistortionK[3]);
            oculusRiftDistortionShader.Parameters["imageScaleFactor"].SetValue(scaleImageFactor);

            // Pass for left lens
            oculusRiftDistortionShader.Parameters["drawLeftLens"].SetValue(true);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, oculusRiftDistortionShader);
            _spriteBatch.Draw(renderTextureLeft, sideBySideLeftSpriteSize, Color.White);
            _spriteBatch.End();

            // Pass for right lens
            oculusRiftDistortionShader.Parameters["drawLeftLens"].SetValue(false);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, oculusRiftDistortionShader);
            _spriteBatch.Draw(renderTextureRight, sideBySideRightSpriteSize, Color.White);
            _spriteBatch.End();
        }
    }
}
