using C3DE.Components;
using C3DE.Graphics.PostProcessing;
using C3DE.UI;
using C3DE.VR;
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
        protected RenderTarget2D[] m_VRRenderTargets = new RenderTarget2D[2];
        protected VRService m_VRService;
        protected internal GUI m_uiManager;
        protected bool m_IsDisposed;
        protected bool m_HDRSupport = false;
        protected bool m_VREnabled;

        #region Properties

        public bool StereoPreview { get; set; } = false;

        public bool Dirty { get; set; } = true;

        public bool VREnabled
        {
            get => m_VREnabled;
            set { SetVREnabled(value); }
        }

        public bool HDRSupport
        {
            get => m_HDRSupport;
            set
            {
                m_HDRSupport = value;
                Dirty = true;
            }
        }

        #endregion

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

        public virtual bool SetVREnabled(bool enabled)
        {
            if (enabled && m_VRService != null || !enabled && m_VRService == null)
                return false;

            var engine = Application.Engine;

            if (enabled)
            {
                var service = VRManager.GetVRAvailableVRService();
                if (service != null)
                {
                    m_VRService = service;
                    m_VREnabled = true;
                    engine.Components.Add(m_VRService);
                }
            }
            else
            {
                if (m_VRService != null)
                {
                    engine.Components.Remove(m_VRService);
                    m_VRService.Dispose();
                }

                m_VREnabled = false;
            }

            Dirty = true;

            VRManager.ActiveService = m_VRService;
            Application.Engine.IsFixedTimeStep = !m_VREnabled;

            return m_VREnabled;
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

        protected abstract void RenderObjects(Scene scene, Camera camera);

        protected abstract void RenderPostProcess(List<PostProcessPass> passes, RenderTarget2D renderTarget);

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
