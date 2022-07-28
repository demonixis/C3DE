using C3DE.Components;
using C3DE.Graphics.Rendering;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.PostProcessing
{
    public abstract class PostProcessPass : IComparable, IDisposable
    {
        protected GraphicsDevice _graphics;
        protected int _order;
        protected bool _VREnabled;
        protected Effect _effect;
        protected RenderTarget2D _mainRenderTarget;
        protected Vector4 _textureSamplerTexelSize;

        public bool Enabled { get; set; } = true;
        public Matrix InverseProjectionMatrix => Matrix.Invert(Camera.Main.ViewMatrix * Camera.Main.ProjectionMatrix);// Matrix.Invert(Camera.Main.ProjectionMatrix);

        public RenderTarget2D GetDepthBuffer()
        {
            var renderer = Application.Engine.Renderer;
            return renderer.GetDepthBuffer();
        }

        public RenderTarget2D GetNormalBuffer()
        {
            var renderer = Application.Engine.Renderer;
            return renderer.GetNormalBuffer();
        }

        public PostProcessPass(GraphicsDevice graphics)
        {
            _graphics = graphics;
            VRManager.VRServiceChanged += OnVRChanged;
        }

        protected virtual void OnVRChanged(VRService service)
        {
            _VREnabled = service != null;
            _mainRenderTarget.Dispose();
            _mainRenderTarget = CreateRenderTarget();
            _textureSamplerTexelSize = new Vector4(1.0f / (float)_mainRenderTarget.Width, 1.0f / (float)_mainRenderTarget.Height, _mainRenderTarget.Width, _mainRenderTarget.Height);
        }

        public virtual void Initialize(ContentManager content)
        {
            _mainRenderTarget = CreateRenderTarget();
            _textureSamplerTexelSize = new Vector4(1.0f / (float)_mainRenderTarget.Width, 1.0f / (float)_mainRenderTarget.Height, _mainRenderTarget.Width, _mainRenderTarget.Height);
        }

        public virtual void SetupEffect()
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch, RenderTarget2D sceneRenderTarget)
        {
            _graphics.SetRenderTarget(_mainRenderTarget);
            _graphics.SamplerStates[1] = SamplerState.LinearClamp;

            SetupEffect();

            DrawFullscreenQuad(spriteBatch, sceneRenderTarget, _mainRenderTarget, _effect);

            _graphics.SetRenderTarget(null);
            _graphics.Textures[1] = _mainRenderTarget;
            _graphics.SetRenderTarget(sceneRenderTarget);

            DrawFullscreenQuad(spriteBatch, _mainRenderTarget, _mainRenderTarget.Width, _mainRenderTarget.Height, null);
        }

        public virtual void Apply(SpriteBatch spriteBatch, RenderTarget2D source, RenderTarget2D destination)
        {
        }

        protected RenderTarget2D CreateRenderTarget(RenderTargetUsage targetUsage = RenderTargetUsage.DiscardContents)
        {
            var pp = _graphics.PresentationParameters;
            var width = pp.BackBufferWidth;
            var height = pp.BackBufferHeight;
            var format = pp.BackBufferFormat;

            if (_VREnabled)
            {
                var size = VRManager.ActiveService.GetRenderTargetSize();
                width = (int)size[0];
                height = (int)size[1];
            }

            return new RenderTarget2D(Application.GraphicsDevice, width, height, false, format, pp.DepthStencilFormat, Math.Max(2, pp.MultiSampleCount), targetUsage);
        }

        protected void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, RenderTarget2D renderTarget, Effect effect)
        {
            _graphics.SetRenderTarget(renderTarget);
            DrawFullscreenQuad(spriteBatch, texture, renderTarget.Width, renderTarget.Height, effect);
        }

        protected void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, int width, int height, Effect effect, BlendState blendState = null)
        {
            spriteBatch.Begin(0, blendState != null ? blendState : BlendState.Opaque, null, null, null, effect);
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }

        public int CompareTo(object obj)
        {
            var pass = obj as PostProcessPass;

            if (pass == null)
                return 1;

            if (_order == pass._order)
                return 0;
            else if (_order > pass._order)
                return 1;
            else
                return -1;
        }

        #region IDisposable

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
        }

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
