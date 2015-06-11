using C3DE.Components;
using C3DE.PostProcess;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Rendering
{
    /// <summary>
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public class Renderer : IRenderer
    {
        private GraphicsDevice _device;
        private RenderTarget2D _sceneRT;
        private SpriteBatch _spriteBatch;
        private bool _needsBufferUpdate;
        internal GUI uiManager;

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
            _spriteBatch = new SpriteBatch(_device);
            uiManager = new GUI(_spriteBatch);
            uiManager.LoadContent(content);
        }

        private void renderShadowMaps(Scene scene, Camera camera)
        {
            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].shadowGenerator.Enabled)
                    scene.Lights[i].shadowGenerator.RenderShadows(_device, scene.renderList);
        }

        /// <summary>
        /// Render renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        private void renderObjects(Scene scene, Camera camera)
        {
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
        }

        /// <summary>
        /// Render buffers to screen.
        /// </summary>
        private void renderBuffers()
        {
            _device.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            _spriteBatch.Draw(_sceneRT, Vector2.Zero, Color.White);
            _spriteBatch.End();
        }

        private void renderPostProcess(List<PostProcessPass> passes)
        {
            if (passes.Count > 0)
            {
                for (int i = 0, l = passes.Count; i < l; i++)
                {
                    if (passes[i].Enabled)
                        passes[i].Apply(_spriteBatch, _sceneRT);
                }
            }
        }

        private void renderUI(List<Behaviour> scripts)
        {
            var size = scripts.Count;

            if (size > 0 && GUI.Enabled)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GUI.uiEffect, GUI.uiMatrix);

                for (int i = 0; i < size; i++)
                    if (scripts[i].Enabled)
                        scripts[i].OnGUI(uiManager);

                _spriteBatch.End();
            }
        }

        /// <summary>
        /// Render the scene with the specified camera.
        /// Render order:
        /// 1 - Shadow maps
        /// 2 - Objects
        /// 3 - PostProcesses
        /// 4 - UI
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

            renderShadowMaps(scene, camera);
            renderObjects(scene, camera);
            renderBuffers();
            renderPostProcess(scene.postProcessPasses);
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
