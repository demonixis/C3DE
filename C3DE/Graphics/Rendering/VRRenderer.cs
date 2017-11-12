using System;
using System.Collections.Generic;
using C3DE.Components;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using C3DE.VR;
using RendererComponent = C3DE.Components.Rendering.Renderer;
using C3DE.Graphics.Materials;

namespace C3DE.Graphics.Rendering
{
    public class VRRenderer : Renderer
    {
        private VRService _vrDevice;
        private RenderTarget2D[] renderTargetEye = new RenderTarget2D[2];
        private Matrix playerMatrix = Matrix.CreateScale(1);

        public bool StereoPreview { get; set; } = false;

        public VRRenderer(GraphicsDevice graphics, VRService vrDevice)
            : base(graphics)
        {
            Application.Engine.IsFixedTimeStep = false;
            _vrDevice = vrDevice;
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            for (int eye = 0; eye < 2; eye++)
                renderTargetEye[eye] = _vrDevice.CreateRenderTargetForEye(eye);
        }

        public override void Render(Scene scene)
        {
            var camera = scene.cameras[0];

            for (int eye = 0; eye < 2; eye++)
            {
                m_graphicsDevice.SetRenderTarget(renderTargetEye[eye]);
                m_graphicsDevice.Clear(Color.Black);

                camera.projection = _vrDevice.GetProjectionMatrix(eye);
				camera.view = _vrDevice.GetViewMatrix(eye, playerMatrix);

				if (camera.transform.Parent != null)
					camera.view *= Matrix.Invert(camera.transform.Parent.world);

                RenderShadowMaps(scene);
                RenderObjects(scene, camera);
                //renderPostProcess(scene.postProcessPasses);
                //RenderUI(scene.Behaviours);
            }

            _vrDevice.SubmitRenderTargets(renderTargetEye[0], renderTargetEye[1]);

            // show left eye view also on the monitor screen 
            DrawEyeViewIntoBackbuffer(0);
        }

        protected override void RenderObjects(Scene scene, Camera camera)
        {
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

        public override void RenderEditor(Scene scene, Camera camera, RenderTarget2D target)
        {
        }

        protected override void RenderPostProcess(List<PostProcessPass> passes)
        {
        }

        private void DrawEyeViewIntoBackbuffer(int eye)
        {
            m_graphicsDevice.SetRenderTarget(null);
            m_graphicsDevice.Clear(Color.Black);

            var pp = m_graphicsDevice.PresentationParameters;
            var height = pp.BackBufferHeight;
            var width = Math.Min(pp.BackBufferWidth, (int)(height * _vrDevice.GetRenderTargetAspectRatio(eye)));
            var offset = (pp.BackBufferWidth - width) / 2;

			m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, _vrDevice.DistortionEffect, null);

			if (StereoPreview || _vrDevice.DistortionCorrectionRequired)
            {
                width = pp.BackBufferWidth / 2;
                m_spriteBatch.Draw(renderTargetEye[0], new Rectangle(0, 0, width, height), null, Color.White, 0, Vector2.Zero, _vrDevice.PreviewRenderEffect, 0);
                _vrDevice.ApplyDistortion(renderTargetEye[0], 0);

                m_spriteBatch.Draw(renderTargetEye[1], new Rectangle(width, 0, width, height), null, Color.White, 0, Vector2.Zero, _vrDevice.PreviewRenderEffect, 0);
                _vrDevice.ApplyDistortion(renderTargetEye[1], 0);
            }
            else
                m_spriteBatch.Draw(renderTargetEye[eye], new Rectangle(offset, 0, width, height), null, Color.White, 0, Vector2.Zero, _vrDevice.PreviewRenderEffect, 0);

            m_spriteBatch.End();
        }

        public override void Dispose(bool disposing)
        {
            if (!m_IsDisposed)
            {
                if (disposing)
                {
                    DisposeObject(renderTargetEye[0]);
                    DisposeObject(renderTargetEye[1]);
                }

                m_IsDisposed = true;
            }
        }
    }
}
