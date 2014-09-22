using C3DE.Components;
using C3DE.PostProcess;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OculusRift.Oculus;
using System;
using System.Collections.Generic;

namespace C3DE
{
    public class OculusRenderer : IRenderer
    {
        private OculusClient oculusClient;
        private Effect oculusRiftDistortionShader;
        private RenderTarget2D renderTargetLeft;
        private RenderTarget2D renderTargetRight;
        private Texture2D renderTextureLeft;
        private Texture2D renderTextureRight;
        private float scaleImageFactor;
        private float fovX;
        private float fovD;
        private static float aspectRatio;
        private float fovY;
        private double resolutionX = 1280;
        private double resolutionY = 800;
        private int viewportWidth;
        private int viewportHeight;
        private Rectangle sideBySideLeftSpriteSize;
        private Rectangle sideBySideRightSpriteSize;

        private GraphicsDevice graphicsDevice;
        private GraphicsDeviceManager graphics;
        private RenderTarget2D _sceneRT;
        private SpriteBatch _spriteBatch;
        private PostProcessManager _postProcessManager;
        private bool _needsBufferUpdate;
        internal GUI _guiManager;

        public bool NeedsBufferUpdate
        {
            get { return _needsBufferUpdate; }
            set { _needsBufferUpdate = value; }
        }

        public OculusRenderer(GraphicsDevice device, GraphicsDeviceManager gdm)
        {
            graphicsDevice = device;
            graphics = gdm;

            _spriteBatch = new SpriteBatch(device);
            _sceneRT = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _postProcessManager = new PostProcessManager();
            _needsBufferUpdate = false;

            oculusClient = new OculusClient();
            scaleImageFactor = 0.71f;

            // PresentationSettings
            graphics.PreferredBackBufferWidth = (int)Math.Ceiling(resolutionX / scaleImageFactor);
            graphics.PreferredBackBufferHeight = (int)Math.Ceiling(resolutionY / scaleImageFactor);

            graphics.IsFullScreen = true;
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
            oculusRiftDistortionShader = Application.Content.Load<Effect>(@"Shader/OculusRift");

            aspectRatio = (float)(OculusClient.GetScreenResolution().X * 0.5f / (float)(OculusClient.GetScreenResolution().Y));
            fovD = OculusClient.GetEyeToScreenDistance();
            fovX = OculusClient.GetScreenSize().Y * scaleImageFactor;
            fovY = 2.0f * (float)Math.Atan(fovX / fovD);

            // Init left and right RenderTarget
            renderTargetLeft = new RenderTarget2D(graphicsDevice, graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight);
            renderTargetRight = new RenderTarget2D(graphicsDevice, graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight);

            OculusClient.SetSensorPredictionTime(0, 0.03f);
            UpdateResolutionAndRenderTargets();
        }

        private void UpdateResolutionAndRenderTargets()
        {
            if (viewportWidth != graphicsDevice.Viewport.Width || viewportHeight != graphicsDevice.Viewport.Height)
            {
                viewportWidth = graphicsDevice.Viewport.Width;
                viewportHeight = graphicsDevice.Viewport.Height;
                sideBySideLeftSpriteSize = new Rectangle(0, 0, viewportWidth / 2, viewportHeight);
                sideBySideRightSpriteSize = new Rectangle(viewportWidth / 2, 0, viewportWidth / 2, viewportHeight);
            }
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

            graphicsDevice.SetRenderTarget(_sceneRT);
            graphicsDevice.Clear(Color.Black);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(graphicsDevice, camera);
            // FIXME !
            // Prepass, Update light, eye position, etc.
           // for (int i = 0; i < scene.effects.Count; i++)
             //   scene.Materials[scene.materialsEffectIndex[i]].PrePass();

            // Pass, Update matrix, material attributes, etc.
            for (int i = 0; i < scene.RenderList.Count; i++)
            {
                if (scene.RenderList[i].Enabled)
                {
                    if (scene.RenderList[i].MaterialCount == 0)
                        scene.DefaultMaterial.Pass(scene.RenderList[i]);
                    else
                        scene.RenderList[i].Material.Pass(scene.RenderList[i]);

                    scene.RenderList[i].Draw(graphicsDevice);
                }
            }

            graphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Render buffers to screen.
        /// </summary>
        private void renderBuffers()
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            _spriteBatch.Draw(_sceneRT, Vector2.Zero, Color.White);
            _spriteBatch.End();
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

        private void renderPostProcess()
        {
            _postProcessManager.Draw(_spriteBatch, _sceneRT);
        }

        private void BaseDraw(Scene scene, Camera camera)
        {
            if (_needsBufferUpdate)
            {
                _sceneRT = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                _needsBufferUpdate = false;
            }

            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].ShadowGenerator.Enabled)
                    scene.Lights[i].ShadowGenerator.RenderShadows(graphicsDevice, scene.RenderList);

            renderObjects(scene, camera);

            renderBuffers();
            //renderPostProcess();
            renderUI(scene.Behaviours);
        }

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        public void render(Scene scene, Camera camera)
        {
            DrawLeftEye();
            BaseDraw(scene, camera);

            DrawRightEye();
            BaseDraw(scene, camera);

            DrawOculusRenderTargets();
        }

        public void RenderEditor(Scene scene, Camera camera, RenderTarget2D target) { }

        private void DrawLeftEye()
        {
            graphicsDevice.SetRenderTarget(renderTargetLeft);
            graphicsDevice.Clear(Color.Black);
        }

        private void DrawRightEye()
        {
            graphicsDevice.SetRenderTarget(renderTargetRight);
            graphicsDevice.Clear(Color.Black);
        }

        private void DrawOculusRenderTargets()
        {
            // Set RenderTargets
            graphicsDevice.SetRenderTarget(null);
            renderTextureLeft = (Texture2D)renderTargetLeft;
            renderTextureRight = (Texture2D)renderTargetRight;
            graphicsDevice.Clear(Color.Black);

            //Set the four Distortion params of the oculus
            oculusRiftDistortionShader.Parameters["distK0"].SetValue(oculusClient.DistK0);
            oculusRiftDistortionShader.Parameters["distK1"].SetValue(oculusClient.DistK1);
            oculusRiftDistortionShader.Parameters["distK2"].SetValue(oculusClient.DistK2);
            oculusRiftDistortionShader.Parameters["distK3"].SetValue(oculusClient.DistK3);
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
