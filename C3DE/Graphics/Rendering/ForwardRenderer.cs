using C3DE.Components;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public class ForwardRenderer : Renderer
    {
        protected RenderTarget2D sceneRT;

        public ForwardRenderer(GraphicsDevice graphics)
           : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            RebuildRenderTargets();
        }

        public override void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                    DisposeObject(sceneRT);

                isDisposed = true;
            }
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
            m_graphicsDevice.BlendState = BlendState.Opaque;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            // Prepass, Update light, eye position, etc.
            for (var i = 0; i < scene.effects.Count; i++)
                scene.materials[scene.materialsEffectIndex[i]].PrePass(camera);

            // Pass, Update matrix, material attributes, etc.
            for (var i = 0; i < renderCount; i++)
            {
                var mat = scene.RenderList[i].Material as Materials.StandardMaterial;
                if (mat != null)
                {
                    scene.renderList[i].Material?.Pass(scene.RenderList[i]);
                    scene.renderList[i].Draw(m_graphicsDevice);

                    m_graphicsDevice.BlendState = BlendState.Additive;

                    for (var l = 0; l < scene.lights.Count; l++)
                    {
                        mat.PassLighting(scene.RenderList[i], scene.lights[l]);
                        scene.renderList[i].Draw(m_graphicsDevice);
                    }

                    m_graphicsDevice.BlendState = BlendState.Opaque;
                }
                else
                {
                    scene.renderList[i].Material?.Pass(scene.RenderList[i]);
                    scene.renderList[i].Draw(m_graphicsDevice);
                }
            }
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

        protected override void RenderPostProcess(List<PostProcessPass> passes)
        {
            if (passes.Count == 0)
                return;

            m_graphicsDevice.SetRenderTarget(sceneRT);

            for (int i = 0, l = passes.Count; i < l; i++)
                if (passes[i].Enabled)
                    passes[i].Draw(m_spriteBatch, sceneRT);
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
            if (scene == null)
                return;

            RebuildRenderTargets();
            RenderSceneForCamera(scene, scene.cameras[0]);
        }

        protected virtual void RebuildRenderTargets()
        {
            if (!NeedsBufferUpdate)
                return;

            sceneRT = new RenderTarget2D(m_graphicsDevice, m_graphicsDevice.Viewport.Width, m_graphicsDevice.Viewport.Height, false, m_HDRSupport ? SurfaceFormat.HdrBlendable : SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
            NeedsBufferUpdate = false;
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera)
        {
            RenderShadowMaps(scene);
            RenderObjects(scene, camera);
            RenderPostProcess(scene.postProcessPasses);
            renderBuffers();
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
