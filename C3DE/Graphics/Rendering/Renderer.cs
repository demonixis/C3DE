using C3DE.Components;
using C3DE.Graphics.PostProcessing;
using C3DE.UI;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Graphics.Rendering
{
    public abstract class Renderer : IDisposable
    {
        protected GraphicsDevice m_graphicsDevice;
        protected SpriteBatch m_spriteBatch;
        protected RenderTarget2D m_SceneRenderTarget;
        protected internal GUI m_uiManager;
        protected bool m_IsDisposed;
        protected bool m_HDRSupport = false;

        public bool NeedsBufferUpdate { get; set; } = true;

        public bool HDRSupport
        {
            get => m_HDRSupport;
            set
            {
                m_HDRSupport = value;
                NeedsBufferUpdate = true;
            }
        }

        public Renderer(GraphicsDevice graphics)
        {
            m_graphicsDevice = graphics;
        }

        public virtual void Initialize(ContentManager content)
        {
            m_spriteBatch = new SpriteBatch(m_graphicsDevice);
            m_uiManager = new GUI(m_spriteBatch);
            m_uiManager.LoadContent(content);
        }

        /// <summary>
        /// Renders shadowmaps.
        /// </summary>
        /// <param name="scene"></param>
        protected virtual void RenderShadowMaps(Scene scene)
        {
            for (int i = 0, l = scene.Lights.Count; i < l; i++)
                if (scene.Lights[i].shadowGenerator.Enabled)
                    scene.Lights[i].shadowGenerator.RenderShadows(m_graphicsDevice, scene.renderList);
        }

        protected virtual void RenderObjects(Scene scene, Camera camera)
        {
            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            // Prepass, Update light, eye position, etc.
            for (var i = 0; i < scene.effects.Count; i++)
                scene.materials[scene.materialsEffectIndex[i]].PrePass(camera);

            // Pass, Update matrix, material attributes, etc.
            for (var i = 0; i < renderCount; i++)
            {
                scene.renderList[i].Material?.Pass(scene.RenderList[i]);
                scene.renderList[i].Draw(m_graphicsDevice);
            }
        }

        protected abstract void RenderPostProcess(List<PostProcessPass> passes);

        protected virtual void RenderUI(List<Behaviour> scripts)
        {
            var size = scripts.Count;
            if (size > 0 && GUI.Enabled)
            {
                m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GUI.uiEffect, GUI.uiMatrix);

                for (int i = 0; i < size; i++)
                    if (scripts[i].Enabled)
                        scripts[i].OnGUI(m_uiManager);

                m_spriteBatch.End();
            }
        }

        /// <summary>
        /// Render the scene.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        public abstract void Render(Scene scene);

        public abstract void RenderEditor(Scene scene, Camera camera, RenderTarget2D target);

        #region IDisposable

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public abstract void Dispose(bool disposing);

        protected void DisposeObject(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }

        #endregion
    }
}
