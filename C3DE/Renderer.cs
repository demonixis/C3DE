using C3DE.Components;
using C3DE.PostProcess;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE
{
    /// <summary>
    /// The renderer will render a scene to the screen according to a camera view.
    /// </summary>
    public class Renderer
    {
        private GraphicsDevice graphicsDevice;
        private RenderTarget2D _sceneRT;
        private SpriteBatch _spriteBatch;
        private PostProcessManager _postProcessManager;
        private Skybox _skybox;
        private bool _needsBufferUpdate;
        internal GUI _guiManager;

        public Skybox Skybox
        {
            get { return _skybox; }
        }

        public bool NeedsBufferUpdate
        {
            get { return _needsBufferUpdate; }
            set { _needsBufferUpdate = value; }
        }

        public Renderer(GraphicsDevice device)
        {
            graphicsDevice = device;
            _spriteBatch = new SpriteBatch(device);
            _sceneRT = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _postProcessManager = new PostProcessManager();
            _skybox = new Skybox();
            _needsBufferUpdate = false;
        }

        public void LoadContent(ContentManager content)
        {
            _guiManager = new GUI(_spriteBatch);
            _guiManager.LoadContent(content);
            _postProcessManager.LoadContent(content);
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

            if (_skybox.Enabled)
                _skybox.Draw(graphicsDevice, camera);

            // Prepass, Update light, eye position, etc.
            for (int i = 0; i < scene.effects.Count; i++)
                scene.materials[scene.materialsEffectIndex[i]].PrePass();

            // Pass, Update matrix, material attributes, etc.
            for (int i = 0; i < scene.RenderList.Count; i++)
            {
                if (scene.renderList[i].Enabled)
                {
                    if (scene.RenderList[i].MaterialCount == 0)
                        scene.RenderList[i].Material = scene.DefaultMaterial;

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

        /// <summary>
        /// Render the scene with the specified camera.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        public void render(Scene scene, Camera camera)
        {
            if (_needsBufferUpdate)
            {
                _sceneRT = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                _needsBufferUpdate = false;
            }

            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].shadowGenerator.Enabled)
                    scene.Lights[i].shadowGenerator.RenderShadows(graphicsDevice, scene.RenderList);

            renderObjects(scene, camera);

            renderBuffers();
            //renderPostProcess();
            renderUI(scene.Behaviours);
        }

        public void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
            if (_needsBufferUpdate)
            {
                _sceneRT = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                _needsBufferUpdate = false;
            }

            renderObjects(scene, camera);
            renderBuffers();

            graphicsDevice.SetRenderTarget(target);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            _spriteBatch.Draw(_sceneRT, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }
}
