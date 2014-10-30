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
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public class Renderer : IRenderer
    {
        private GraphicsDevice _device;
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

        public Renderer()
        {
            _needsBufferUpdate = false;
        }

        public void Initialize(ContentManager content)
        {
            _device = Application.GraphicsDevice;
            _sceneRT = new RenderTarget2D(_device, _device.Viewport.Width, _device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            _postProcessManager = new PostProcessManager();
            _spriteBatch = new SpriteBatch(_device);
            _guiManager = new GUI(_spriteBatch);
            _guiManager.LoadContent(content);
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

            _device.SetRenderTarget(_sceneRT);
            _device.Clear(Color.Black);
            _device.DepthStencilState = DepthStencilState.Default;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(_device, camera);

            // Prepass, Update light, eye position, etc.
            for (int i = 0; i < scene.effects.Count; i++)
                scene.materials[scene.materialsEffectIndex[i]].PrePass();

            // Pass, Update matrix, material attributes, etc.
            for (int i = 0; i < scene.RenderList.Count; i++)
            {
                if (scene.renderList[i].Enabled)
                {
                    if (scene.RenderList[i].MaterialCount == 0)
                        scene.DefaultMaterial.Pass(scene.renderList[i]);
                    else
                        scene.RenderList[i].Material.Pass(scene.RenderList[i]);
                    
                    scene.RenderList[i].Draw(_device);
                }
            }

            _device.SetRenderTarget(null);
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
                _sceneRT = new RenderTarget2D(_device, _device.Viewport.Width, _device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                _needsBufferUpdate = false;
            }

            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].shadowGenerator.Enabled)
                    scene.Lights[i].shadowGenerator.RenderShadows(_device, scene.RenderList);

            renderObjects(scene, camera);

            renderBuffers();
            //renderPostProcess();
            renderUI(scene.Behaviours);
        }

        public void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
            if (_needsBufferUpdate)
            {
                _sceneRT = new RenderTarget2D(_device, _device.Viewport.Width, _device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                _needsBufferUpdate = false;
            }

            renderObjects(scene, camera);
            renderBuffers();

            _device.SetRenderTarget(target);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            _spriteBatch.Draw(_sceneRT, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }
    }
}
