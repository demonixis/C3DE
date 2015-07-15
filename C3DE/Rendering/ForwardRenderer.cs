using C3DE.Components;
using C3DE.Components.Renderers;
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
    public class ForwardRenderer : IRenderer
    {
        protected GraphicsDevice graphicsDevice;
        protected RenderTarget2D sceneRT;
        protected SpriteBatch spriteBatch;
        protected bool needsBufferUpdate;
        protected internal GUI uiManager;

        public bool NeedsBufferUpdate
        {
            get { return needsBufferUpdate; }
            set { needsBufferUpdate = value; }
        }

        public ForwardRenderer()
        {
            needsBufferUpdate = false;
        }

        public virtual void Initialize(ContentManager content)
        {
            graphicsDevice = Application.GraphicsDevice;
            sceneRT = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            spriteBatch = new SpriteBatch(graphicsDevice);
            uiManager = new GUI(spriteBatch);
            uiManager.LoadContent(content);
        }

        protected virtual void renderShadowMaps(Scene scene, Camera camera)
        {
            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].shadowGenerator.Enabled)
                    scene.Lights[i].shadowGenerator.RenderShadows(graphicsDevice, scene.renderList);
        }

        /// <summary>
        /// Render renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        protected void renderObjects(Scene scene, Camera camera)
        {
            graphicsDevice.SetRenderTarget(sceneRT);
            graphicsDevice.Clear(camera.clearColor);
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(graphicsDevice, camera);

            // Prepass, Update light, eye position, etc.
            for (int i = 0; i < scene.effects.Count; i++)
                scene.materials[scene.materialsEffectIndex[i]].PrePass(camera);

            // Pass, Update matrix, material attributes, etc.
            // Note: The renderList contains ONLY enabled components/objects.
            for (int i = 0; i < scene.RenderList.Count; i++)
            {
                scene.RenderList[i].Material.Pass(scene.RenderList[i]);
                scene.RenderList[i].Draw(graphicsDevice);
            }
        }

        /// <summary>
        /// Render buffers to screen.
        /// </summary>
        protected void renderBuffers()
        {
            graphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(sceneRT, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

        protected void renderPostProcess(List<PostProcessPass> passes)
        {
            if (passes.Count > 0)
            {
                for (int i = 0, l = passes.Count; i < l; i++)
                {
                    if (passes[i].Enabled)
                        passes[i].Apply(spriteBatch, sceneRT);
                }
            }
        }

        protected void renderUI(List<Behaviour> scripts)
        {
            var size = scripts.Count;

            if (size > 0 && GUI.Enabled)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GUI.uiEffect, GUI.uiMatrix);

                for (int i = 0; i < size; i++)
                    if (scripts[i].Enabled)
                        scripts[i].OnGUI(uiManager);

                spriteBatch.End();
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
        public virtual void Render(Scene scene)
        {
            RebuildRenderTargets();
            RenderSceneForCamera(scene, scene.cameras[0]);
        }

        protected virtual void RebuildRenderTargets()
        {
            if (needsBufferUpdate)
            {
                sceneRT = new RenderTarget2D(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                needsBufferUpdate = false;
            }
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera)
        {
            renderShadowMaps(scene, camera);
            renderObjects(scene, camera);
            renderBuffers();
            renderPostProcess(scene.postProcessPasses);
            renderUI(scene.Behaviours);
        }

        public void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
            RebuildRenderTargets();

            renderShadowMaps(scene, camera);
            renderObjects(scene, camera);
            renderBuffers();
            //renderPostProcess(scene.PostProcessPasses);
            //renderUI(scene.Behaviours);

            graphicsDevice.SetRenderTarget(target);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(sceneRT, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}
