using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Materials.Shaders;
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
    public class ForwardRenderer : BaseRenderer
    {
        private DepthRenderer m_DepthRenderer;

        public DepthRenderer DepthRenderer => m_DepthRenderer;

        public ForwardRenderer(GraphicsDevice graphics)
           : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            RebuildRenderTargets();
            m_DepthRenderer = new DepthRenderer();
        }

        public override void Dispose(bool disposing)
        {
            if (!m_IsDisposed)
            {
                if (disposing)
                {
                    for (var eye = 0; eye < 2; eye++)
                        DisposeObject(m_SceneRenderTargets[eye]);
                }
                m_IsDisposed = true;
            }
        }

        /// <summary>
        /// Renders the scene with the specified camera.
        /// Render order:
        /// 1 - Shadow maps
        /// 2 - Objects
        /// 3 - Post Processing
        /// 4 - UI
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        public override void Render(Scene scene)
        {
            if (scene == null || scene?.cameras.Count == 0)
                return;

            var camera = scene.cameras[0];

            RebuildRenderTargets();

            RenderShadowMaps(scene);

            m_DepthRenderer.Draw(m_graphicsDevice);

            if (m_VREnabled)
            {
                for (var eye = 0; eye < 2; eye++)
                {
                    camera.m_ProjectionMatrix = m_VRService.GetProjectionMatrix(eye);
                    camera.m_ViewMatrix = m_VRService.GetViewMatrix(eye, Matrix.Identity);

                    RenderSceneForCamera(scene, camera, m_SceneRenderTargets[eye]);
                    m_VRService.SubmitRenderTarget(eye, m_SceneRenderTargets[eye]);
                }

                //m_VRService.SubmitRenderTargets(m_SceneRenderTargets[0], m_SceneRenderTargets[1]);
                DrawVRPreview(0);
            }
            else
                RenderSceneForCamera(scene, camera, m_SceneRenderTargets[0]);
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, RenderTarget2D renderTarget)
        {
            m_graphicsDevice.SetRenderTarget(renderTarget);
            m_graphicsDevice.Clear(camera.clearColor);

            RenderObjects(scene, camera);
            RenderPostProcess(scene.postProcessPasses, renderTarget);

            if (!m_VREnabled)
            {
                RenderBuffers();
                RenderUI(scene.Behaviours);
            }
        }

        /// <summary>
        /// Renders renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        protected void RenderObjects(Scene scene, Camera camera)
        {
            m_graphicsDevice.DepthStencilState = DepthStencilState.Default;
            m_graphicsDevice.BlendState = BlendState.Opaque;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            Renderer renderer;
            Material material;
            ShaderMaterial shader;
            IMultipassLightingMaterial multiLightShader;
            IEmissiveMaterial emissiveShader;
            var lights = scene.lights;
            var lightCount = lights.Count;

            // Pass, Update matrix, material attributes, etc.
            for (var i = 0; i < renderCount; i++)
            {
                renderer = scene.renderList[i];
                material = scene.renderList[i].Material;

                // A specific renderer that uses its own draw logic.
                if (material == null)
                {
                    renderer.Draw(m_graphicsDevice);
                    continue;
                }

                shader = material.m_ShaderMaterial;

                // Ambient pass
                shader.PrePass(camera);
                shader.Pass(scene.RenderList[i]);
                renderer.Draw(m_graphicsDevice);

                // Lightpass
                if (shader is IMultipassLightingMaterial)
                {
                    multiLightShader = (IMultipassLightingMaterial)shader;

                    m_graphicsDevice.BlendState = BlendState.Additive;

                    for (var l = 0; l < lightCount; l++)
                    {
                        multiLightShader.LightPass(renderer, lights[l]);
                        renderer.Draw(m_graphicsDevice);
                    }

                    m_graphicsDevice.BlendState = BlendState.Opaque;
                }

                // Emissive pass
                if (shader is IEmissiveMaterial)
                {
                    emissiveShader = (IEmissiveMaterial)shader;

                    if (!emissiveShader.EmissiveEnabled)
                        continue;

                    m_graphicsDevice.BlendState = BlendState.Additive;

                    for (var l = 0; l < scene.lights.Count; l++)
                    {
                        emissiveShader.EmissivePass(renderer);
                        renderer.Draw(m_graphicsDevice);
                    }

                    m_graphicsDevice.BlendState = BlendState.Opaque;
                }
            }
        }

        /// <summary>
        /// Renders buffers to screen.
        /// </summary>
        protected virtual void RenderBuffers()
        {
            m_graphicsDevice.SetRenderTarget(null);
            m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            m_spriteBatch.Draw(m_SceneRenderTargets[0], Vector2.Zero, Color.White);
            m_spriteBatch.End();
        }

        /// <summary>
        /// Renders effects.
        /// </summary>
        /// <param name="passes"></param>
        /// <param name="renderTarget"></param>
        protected void RenderPostProcess(List<PostProcessPass> passes, RenderTarget2D renderTarget)
        {
            if (passes.Count == 0)
                return;

            m_graphicsDevice.SetRenderTarget(renderTarget);

            for (int i = 0, l = passes.Count; i < l; i++)
                if (passes[i].Enabled)
                    passes[i].Draw(m_spriteBatch, renderTarget);
        }
    }
}
