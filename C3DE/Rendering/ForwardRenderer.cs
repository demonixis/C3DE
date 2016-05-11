using C3DE.Components;
using C3DE.PostProcess;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Rendering
{
    /// <summary>
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public class ForwardRenderer : Renderer
    {
        protected RenderTarget2D sceneRT;

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            sceneRT = new RenderTarget2D(m_graphicsDevice, m_graphicsDevice.Viewport.Width, m_graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        }

        /// <summary>
        /// Render renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        protected override void RenderObjects(Scene scene, Camera camera)
        {
            m_graphicsDevice.SetRenderTarget(sceneRT);
            m_graphicsDevice.Clear(camera.clearColor);
            m_graphicsDevice.DepthStencilState = DepthStencilState.Default;

            base.RenderObjects(scene, camera);
        }

        /// <summary>
        /// Render buffers to screen.
        /// </summary>
        protected virtual void renderBuffers()
        {
            m_graphicsDevice.SetRenderTarget(null);
            m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            m_spriteBatch.Draw(sceneRT, Vector2.Zero, Color.White);
            m_spriteBatch.End();
        }

        protected override void renderPostProcess(List<PostProcessPass> passes)
        {
            if (passes.Count > 0)
            {
                for (int i = 0, l = passes.Count; i < l; i++)
                {
                    if (passes[i].Enabled)
                        passes[i].Apply(m_spriteBatch, sceneRT);
                }
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
        public override void Render(Scene scene)
        {
            if (scene != null)
            {
                RebuildRenderTargets();
                RenderSceneForCamera(scene, scene.cameras[0]);
            }
        }

        protected virtual void RebuildRenderTargets()
        {
            if (NeedsBufferUpdate)
            {
                sceneRT = new RenderTarget2D(m_graphicsDevice, m_graphicsDevice.Viewport.Width, m_graphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
                NeedsBufferUpdate = false;
            }
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera)
        {
            RenderShadowMaps(scene);
            RenderObjects(scene, camera);
            renderBuffers();
            renderPostProcess(scene.postProcessPasses);
            RenderUI(scene.Behaviours);
        }

        public override void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
            RebuildRenderTargets();

            RenderShadowMaps(scene);
            RenderObjects(scene, camera);
            renderBuffers();
            //renderPostProcess(scene.PostProcessPasses);
            //renderUI(scene.Behaviours);

            m_graphicsDevice.SetRenderTarget(target);
            m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            m_spriteBatch.Draw(sceneRT, Vector2.Zero, Color.White);
            m_spriteBatch.End();
        }
    }
}
