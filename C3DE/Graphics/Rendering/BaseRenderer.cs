using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Graphics.PostProcessing;
using C3DE.UI;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Graphics.Rendering
{
    public abstract class BaseRenderer : IDisposable
    {
        protected internal GraphicsDevice _graphicsDevice;
        protected SpriteBatch m_spriteBatch;
        protected internal RenderTarget2D[] _sceneRenderTargets = new RenderTarget2D[2];
        protected VRService m_VRService;
        protected Light m_AmbientLight;
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

        public BaseRenderer(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;
        }

        public virtual void Initialize(ContentManager content)
        {
            m_spriteBatch = new SpriteBatch(_graphicsDevice);
            m_uiManager = new GUI(m_spriteBatch);
            m_uiManager.LoadContent(content);

            m_AmbientLight = new Light();
            m_AmbientLight.TypeLight = LightType.Ambient;
            m_AmbientLight.Start();
        }

        protected RenderTarget2D CreateRenderTarget(SurfaceFormat surfaceFormat = SurfaceFormat.Color, DepthFormat depthFormat = DepthFormat.Depth24, bool mipMap = false, int preferredMultiSampleCount = -1, RenderTargetUsage usage = RenderTargetUsage.DiscardContents)
        {
            var width = _graphicsDevice.PresentationParameters.BackBufferWidth;
            var height = _graphicsDevice.PresentationParameters.BackBufferHeight;

            if (preferredMultiSampleCount == -1)
            {
                if (m_VREnabled)
                    preferredMultiSampleCount = 0;
                else
                    preferredMultiSampleCount = _graphicsDevice.PresentationParameters.MultiSampleCount;
            }

            if (m_VREnabled)
            {
                width = _sceneRenderTargets[0].Width;
                height = _sceneRenderTargets[0].Height;
            }

            return new RenderTarget2D(_graphicsDevice, width, height, mipMap, surfaceFormat, depthFormat, preferredMultiSampleCount, usage);
        }

        /// <summary>
        /// Rebuilds render targets if Dirty is true.
        /// </summary>
        protected virtual void RebuildRenderTargets()
        {
            if (!Dirty)
                return;

            for (var eye = 0; eye < 2; eye++)
                _sceneRenderTargets[eye]?.Dispose();

            if (m_VREnabled)
            {
                for (var eye = 0; eye < 2; eye++)
                    _sceneRenderTargets[eye] = m_VRService.CreateRenderTargetForEye(eye);
            }
            else
            {
                var pp = _graphicsDevice.PresentationParameters;
                var surfaceFormat = m_HDRSupport ? SurfaceFormat.HdrBlendable : pp.BackBufferFormat;
                _sceneRenderTargets[0] = new RenderTarget2D(_graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, surfaceFormat, pp.DepthStencilFormat, pp.MultiSampleCount, RenderTargetUsage.DiscardContents);
            }

            Dirty = false;
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
                    if (m_VRService != null)
                    {
                        engine.Components.Remove(m_VRService);
                        m_VRService.Dispose();
                    }

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
                    m_VRService = null;
                }

                m_VREnabled = false;
            }

            Dirty = true;

            VRManager.ActiveService = m_VRService;

            if (m_VREnabled)
            {
                Application.Engine.IsFixedTimeStep = false;
                Application.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
                Application.GraphicsDevice.PresentationParameters.PresentationInterval = PresentInterval.Immediate;
            }


            return m_VREnabled;
        }

        /// <summary>
        /// Renders shadowmaps.
        /// </summary>
        /// <param name="scene"></param>
        protected virtual void RenderShadowMaps(Scene scene)
        {
            foreach (var light in scene.lights)
            {
                if (light.ShadowEnabled)
                    light._shadowGenerator.RenderShadows(_graphicsDevice, scene.renderList, light);
            }
        }

        /// <summary>
        /// Renders buffers to screen.
        /// </summary>
        protected virtual void RenderToBackBuffer()
        {
#if !ANDROID
            _graphicsDevice.SetRenderTarget(null);
#endif
            m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            m_spriteBatch.Draw(_sceneRenderTargets[0], Vector2.Zero, Color.White);
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

            _graphicsDevice.SetRenderTarget(renderTarget);

            for (int i = 0, l = passes.Count; i < l; i++)
                if (passes[i].Enabled)
                    passes[i].Draw(m_spriteBatch, renderTarget);
        }

        protected virtual void RenderUI(List<Behaviour> scripts)
        {
            var size = scripts.Count;
            if (size > 0 && GUI.Enabled)
            {
                m_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GUI.uiEffect, GUI.uiMatrix);

                for (int i = 0; i < size; i++)
                {
                    // FIXME: We can't access the script if it's disabled
                    if (size != scripts.Count)
                    {
                        m_spriteBatch.End();
                        return;
                    }

                    if (scripts[i].Enabled)
                        scripts[i].OnGUI(m_uiManager);
                }
                m_spriteBatch.End();
            }
        }

        /// <summary>
        /// Render the scene.
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        public abstract void Render(Scene scene);

        public abstract void RenderReflectionProbe(Camera camera);

        /// <summary>
        /// Draws the VR Preview to the Back Buffer
        /// </summary>
        /// <param name="eye"></param>
        protected virtual void DrawVRPreview(int eye)
        {
            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.Clear(Color.Black);

            var pp = _graphicsDevice.PresentationParameters;
            var height = pp.BackBufferHeight;
            var width = MathHelper.Min(pp.BackBufferWidth, (int)(height * m_VRService.GetRenderTargetAspectRatio(eye)));
            var offset = (pp.BackBufferWidth - width) / 2;

            m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, m_VRService.DistortionEffect, null);

            if (StereoPreview || m_VRService.DistortionCorrectionRequired)
            {
                width = pp.BackBufferWidth / 2;
                m_spriteBatch.Draw(_sceneRenderTargets[0], new Rectangle(0, 0, width, height), null, Color.White, 0, Vector2.Zero, m_VRService.PreviewRenderEffect, 0);
                m_VRService.ApplyDistortion(_sceneRenderTargets[0], 0);

                m_spriteBatch.Draw(_sceneRenderTargets[1], new Rectangle(width, 0, width, height), null, Color.White, 0, Vector2.Zero, m_VRService.PreviewRenderEffect, 0);
                m_VRService.ApplyDistortion(_sceneRenderTargets[1], 0);
            }
            else
                m_spriteBatch.Draw(_sceneRenderTargets[eye], new Rectangle(offset, 0, width, height), null, Color.White, 0, Vector2.Zero, m_VRService.PreviewRenderEffect, 0);

            m_spriteBatch.End();
        }

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

        protected void DisposeObject(IDisposable[] disposables)
        {
            if (disposables != null)
            {
                for (var i = 0; i < disposables.Length; i++)
                {
                    if (disposables[i] == null)
                        continue;

                    disposables[i].Dispose();
                    disposables[i] = null;
                }
            }
        }

#endregion
    }
}
