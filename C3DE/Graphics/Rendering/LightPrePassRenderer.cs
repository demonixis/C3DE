using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Graphics.Rendering
{
    public class LightPrePassRenderer : BaseRenderer
    {
        internal RenderTarget2D m_DepthRT;
        internal RenderTarget2D m_NormalRT;
        internal RenderTarget2D m_LightRT;
        private Effect m_DepthNormalEffect;

        public RenderTarget2D DepthBuffer => m_DepthRT;
        public RenderTarget2D NormalBuffer => m_NormalRT;
        public RenderTarget2D LightBuffer => m_LightRT;

        public LightPrePassRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            m_DepthNormalEffect = content.Load<Effect>("Shaders/LPP/DepthNormal");
        }

        protected override void RebuildRenderTargets()
        {
            if (!Dirty)
                return;

            base.RebuildRenderTargets();

            m_DepthRT = CreateRenderTarget();
            m_NormalRT = CreateRenderTarget();
            m_LightRT = CreateRenderTarget();
        }

        public override void Dispose(bool disposing)
        {
            if (!m_IsDisposed)
            {
                if (disposing)
                {
                    for (var eye = 0; eye < 2; eye++)
                        DisposeObject(_sceneRenderTargets[eye]);

                    DisposeObject(m_DepthRT);
                    DisposeObject(m_NormalRT);
                    DisposeObject(m_LightRT);
                    DisposeObject(_sceneRenderTargets);
                }
                m_IsDisposed = true;
            }
        }

        private void DrawDepthNormalMap(Scene scene, Camera camera, int eye)
        {
            _graphicsDevice.SetRenderTargets(m_NormalRT, m_DepthRT);
            _graphicsDevice.Clear(Color.White);

            m_DepthNormalEffect.Parameters["View"].SetValue(camera._viewMatrix);
            m_DepthNormalEffect.Parameters["Projection"].SetValue(camera._projectionMatrix);

            for (int i = 0, l = scene.renderList.Count; i < l; i++)
            {
                m_DepthNormalEffect.Parameters["World"].SetValue(scene.renderList[i].Transform._worldMatrix);
                m_DepthNormalEffect.CurrentTechnique.Passes[0].Apply();
                scene.renderList[i].Draw(_graphicsDevice);
            }

            _graphicsDevice.SetRenderTarget(null);
        }

        private void DrawLightMap(Scene scene, Camera camera, int eye)
        {
            _graphicsDevice.SetRenderTarget(m_LightRT);
            _graphicsDevice.Clear(Color.Transparent);

            m_AmbientLight.Color = Scene.current.RenderSettings.AmbientColor;
            m_AmbientLight.RenderLPP(m_NormalRT, m_DepthRT, camera);

            for (var i = 0; i < scene.lights.Count; i++)
                scene.lights[i].RenderLPP(m_NormalRT, m_DepthRT, camera);

            _graphicsDevice.SetRenderTarget(null);
        }

        private void DrawObjects(Scene scene, Camera camera, int eye)
        {
            _graphicsDevice.SetRenderTarget(_sceneRenderTargets[eye]);
            _graphicsDevice.Clear(camera._clearColor);

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            Renderer renderer;
            Material material;
            LPPShader shader;

            for (var i = 0; i < renderCount; i++)
            {
                renderer = scene.renderList[i];
                material = scene.renderList[i].Material;

                // A specific renderer that uses its own draw logic.
                if (material == null)
                {
                    renderer.Draw(_graphicsDevice);
                    continue;
                }

                shader = (LPPShader)material._shaderMaterial;

                // Ambient pass
                shader.PrePass(camera);
                shader.Pass(scene.RenderList[i], m_LightRT);
                renderer.Draw(_graphicsDevice);
            }
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, int eye)
        {
            using (_graphicsDevice.GeometryState())
                DrawDepthNormalMap(scene, camera, eye);

            using (_graphicsDevice.LightPrePassState())
                DrawLightMap(scene, camera, eye);

            using (_graphicsDevice.GeometryState())
                DrawObjects(scene, camera, eye);

            using (_graphicsDevice.PostProcessState())
            {
                RenderPostProcess(scene.postProcessPasses, _sceneRenderTargets[eye]);

                if (!m_VREnabled)
                {
                    RenderToBackBuffer();
                    RenderUI(scene.Behaviours);
                }
            }
        }

        public override void Render(Scene scene)
        {
            if (scene == null || scene?.cameras.Count == 0)
                return;

            var camera = scene.cameras[0];

            RebuildRenderTargets();
            RenderShadowMaps(scene);

            if (m_VREnabled)
            {
                var cameraParent = Matrix.Identity;
                var parent = camera._transform.Parent;
                if (parent != null)
                    cameraParent = parent._worldMatrix;

                for (var eye = 0; eye < 2; eye++)
                {
                    camera._projectionMatrix = m_VRService.GetProjectionMatrix(eye);
                    camera._viewMatrix = m_VRService.GetViewMatrix(eye, cameraParent);
                    RenderSceneForCamera(scene, camera, eye);
                }

                m_VRService.SubmitRenderTargets(_sceneRenderTargets[0], _sceneRenderTargets[1]);
                DrawVRPreview(0);
                RenderUI(scene.Behaviours);
            }
            else
                RenderSceneForCamera(scene, camera, 0);
        }

        public override void RenderReflectionProbe(Camera camera)
        {
        }
    }
}

