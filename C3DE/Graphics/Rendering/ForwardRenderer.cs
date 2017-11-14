using C3DE.Components;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using RendererComponent = C3DE.Components.Rendering.Renderer;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public class ForwardRenderer : Renderer
    {
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
            if (!m_IsDisposed)
            {
                if (disposing)
                {
                    DisposeObject(m_SceneRenderTarget);

                    for (var eye = 0; eye < 2; eye++)
                        DisposeObject(m_VRRenderTargets[eye]);
                }
                m_IsDisposed = true;
            }
        }

        /// <summary>
        /// Rebuilds render targets if Dirty is true.
        /// </summary>
        protected virtual void RebuildRenderTargets()
        {
            if (!Dirty)
                return;

            m_SceneRenderTarget?.Dispose();

            for (var eye = 0; eye < 2; eye++)
                m_VRRenderTargets[eye]?.Dispose();

            if (m_VREnabled)
            {
                for (var eye = 0; eye < 2; eye++)
                    m_VRRenderTargets[eye] = m_VRService.CreateRenderTargetForEye(eye);
            }
            else
                m_SceneRenderTarget = new RenderTarget2D(m_graphicsDevice, m_graphicsDevice.Viewport.Width, m_graphicsDevice.Viewport.Height, false, m_HDRSupport ? SurfaceFormat.HdrBlendable : SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);

            Dirty = false;
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

            if (m_VREnabled)
            {
                for (var eye = 0; eye < 2; eye++)
                {
                    camera.projection = m_VRService.GetProjectionMatrix(eye);
                    camera.view = m_VRService.GetViewMatrix(eye, Matrix.Identity);

                    RenderSceneForCamera(scene, camera, m_VRRenderTargets[eye]);
                }

                m_VRService.SubmitRenderTargets(m_VRRenderTargets[0], m_VRRenderTargets[1]);
                DrawVRPreview(0);
            }
            else
                RenderSceneForCamera(scene, camera, m_SceneRenderTarget);
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, RenderTarget2D renderTarget)
        {
            m_graphicsDevice.SetRenderTarget(renderTarget);
            m_graphicsDevice.Clear(camera.clearColor);

            RenderShadowMaps(scene);
            RenderObjects(scene, camera);
            RenderPostProcess(scene.postProcessPasses, renderTarget);

            if (!m_VREnabled)
            {
                renderBuffers();
                RenderUI(scene.Behaviours);
            }
        }

        /// <summary>
        /// Renders renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        protected override void RenderObjects(Scene scene, Camera camera)
        {
            m_graphicsDevice.DepthStencilState = DepthStencilState.Default;
            m_graphicsDevice.BlendState = BlendState.Opaque;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            // Prepass, Update light, eye position, etc.
            for (var i = 0; i < scene.effects.Count; i++)
                scene.materials[scene.materialsEffectIndex[i]].PrePass(camera);

            RendererComponent renderer;
            Material material;
            IMultipassLightingMaterial lightMaterial;
            IEmissiveMaterial emissiveMaterial;
            var lights = scene.lights;
            var lightCount = lights.Count;

            // Pass, Update matrix, material attributes, etc.
            for (var i = 0; i < renderCount; i++)
            {
                renderer = scene.renderList[i];
                material = scene.renderList[i].Material;

                // Ambient pass
                material?.Pass(scene.RenderList[i]);
                renderer.Draw(m_graphicsDevice);

                // Lightpass
                if (material is IMultipassLightingMaterial)
                {
                    lightMaterial = (IMultipassLightingMaterial)material;

                    m_graphicsDevice.BlendState = BlendState.Additive;

                    for (var l = 0; l < lightCount; l++)
                    {
                        lightMaterial.LightPass(renderer, lights[l]);
                        renderer.Draw(m_graphicsDevice);
                    }

                    m_graphicsDevice.BlendState = BlendState.Opaque;
                }

                // Emissive pass
                if (material is IEmissiveMaterial)
                {
                    emissiveMaterial = (IEmissiveMaterial)material;

                    if (!emissiveMaterial.EmissiveEnabled)
                        continue;

                    m_graphicsDevice.BlendState = BlendState.Additive;

                    for (var l = 0; l < scene.lights.Count; l++)
                    {
                        emissiveMaterial.EmissivePass(renderer);
                        renderer.Draw(m_graphicsDevice);
                    }

                    m_graphicsDevice.BlendState = BlendState.Opaque;
                }
            }
        }

        /// <summary>
        /// Renders buffers to screen.
        /// </summary>
        protected virtual void renderBuffers()
        {
            m_graphicsDevice.SetRenderTarget(null);
            m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            m_spriteBatch.Draw(m_SceneRenderTarget, Vector2.Zero, Color.White);
            m_spriteBatch.End();
        }

        /// <summary>
        /// Renders effects.
        /// </summary>
        /// <param name="passes"></param>
        /// <param name="renderTarget"></param>
        protected override void RenderPostProcess(List<PostProcessPass> passes, RenderTarget2D renderTarget)
        {
            if (passes.Count == 0)
                return;

            m_graphicsDevice.SetRenderTarget(renderTarget);

            for (int i = 0, l = passes.Count; i < l; i++)
                if (passes[i].Enabled)
                    passes[i].Draw(m_spriteBatch, renderTarget);
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="camera"></param>
        /// <param name="target"></param>
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
            m_spriteBatch.Draw(m_SceneRenderTarget, Vector2.Zero, Color.White);
            m_spriteBatch.End();
        }

        /// <summary>
        /// Draws the VR Preview to the Back Buffer
        /// </summary>
        /// <param name="eye"></param>
        private void DrawVRPreview(int eye)
        {
            m_graphicsDevice.SetRenderTarget(null);
            m_graphicsDevice.Clear(Color.Black);

            var pp = m_graphicsDevice.PresentationParameters;
            var height = pp.BackBufferHeight;
            var width = MathHelper.Min(pp.BackBufferWidth, (int)(height * m_VRService.GetRenderTargetAspectRatio(eye)));
            var offset = (pp.BackBufferWidth - width) / 2;

            m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, m_VRService.DistortionEffect, null);

            if (StereoPreview || m_VRService.DistortionCorrectionRequired)
            {
                width = pp.BackBufferWidth / 2;
                m_spriteBatch.Draw(m_VRRenderTargets[0], new Rectangle(0, 0, width, height), null, Color.White, 0, Vector2.Zero, m_VRService.PreviewRenderEffect, 0);
                m_VRService.ApplyDistortion(m_VRRenderTargets[0], 0);

                m_spriteBatch.Draw(m_VRRenderTargets[1], new Rectangle(width, 0, width, height), null, Color.White, 0, Vector2.Zero, m_VRService.PreviewRenderEffect, 0);
                m_VRService.ApplyDistortion(m_VRRenderTargets[1], 0);
            }
            else
                m_spriteBatch.Draw(m_VRRenderTargets[eye], new Rectangle(offset, 0, width, height), null, Color.White, 0, Vector2.Zero, m_VRService.PreviewRenderEffect, 0);

            m_spriteBatch.End();
        }
    }
}
