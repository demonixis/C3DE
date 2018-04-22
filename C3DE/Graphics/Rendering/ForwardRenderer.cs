using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Materials.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
                    DisposeObject(m_SceneRenderTargets);

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
        public override void Render(Scene scene, Camera camera = null)
        {
            if (scene == null || scene?.cameras.Count == 0)
                return;

            if (camera == null)
            {
                camera = scene.cameras[0];

                if (scene.m_ReflectionProbes.Count > 0)
                    for (var i = 0; i < scene.m_ReflectionProbes.Count; i++)
                        scene.m_ReflectionProbes[i].Draw(this);
            }

            RebuildRenderTargets();

            RenderShadowMaps(scene);

            if (m_VREnabled)
            {
                // Apply camera parenting
                var cameraParent = Matrix.Identity;
                var parent = camera.m_Transform.Parent;
                if (parent != null)
                    cameraParent = parent.m_WorldMatrix;

                for (var eye = 0; eye < 2; eye++)
                {
                    camera.m_ProjectionMatrix = m_VRService.GetProjectionMatrix(eye);
                    camera.m_ViewMatrix = m_VRService.GetViewMatrix(eye, cameraParent);
                    RenderSceneForCamera(scene, camera, m_SceneRenderTargets[eye]);
                }

                m_VRService.SubmitRenderTargets(m_SceneRenderTargets[0], m_SceneRenderTargets[1]);
                DrawVRPreview(0);
                RenderUI(scene.Behaviours);
            }
            else
                RenderSceneForCamera(scene, camera, m_SceneRenderTargets[0]);
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, RenderTarget2D renderTarget)
        {
            if (m_DepthRenderer.Enabled)
                m_DepthRenderer.Draw(m_graphicsDevice);

            m_graphicsDevice.SetRenderTarget(renderTarget);

            var renderToRT = camera.RenderTarget != null;
            if (renderToRT)
                m_graphicsDevice.SetRenderTarget(camera.RenderTarget);

            m_graphicsDevice.Clear(camera.clearColor);

            RenderObjects(scene, camera);
            RenderPostProcess(scene.postProcessPasses, renderTarget);

            if (renderToRT)
                return;

            if (!m_VREnabled)
            {
                RenderToBackBuffer();
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
            }
        }
    }
}
