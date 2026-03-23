using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Graphics.PostProcessing
{
    internal sealed class PostProcessRenderTargetPool
    {
        private readonly GraphicsDevice _graphics;
        private readonly List<PooledRenderTarget> _targets;

        public PostProcessRenderTargetPool(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _targets = new List<PooledRenderTarget>(8);
        }

        public RenderTarget2D Rent(int width, int height, SurfaceFormat format)
        {
            for (var i = 0; i < _targets.Count; i++)
            {
                if (_targets[i].TryRent(width, height, format, out var target))
                    return target;
            }

            var created = Create(width, height, format);
            _targets.Add(new PooledRenderTarget(created));
            return created;
        }

        public void Release(RenderTarget2D renderTarget)
        {
            for (var i = 0; i < _targets.Count; i++)
            {
                if (_targets[i].RenderTarget == renderTarget)
                {
                    _targets[i].IsActive = false;
                    break;
                }
            }
        }

        public void Reset()
        {
            for (var i = 0; i < _targets.Count; i++)
                _targets[i].IsActive = false;
        }

        private RenderTarget2D Create(int width, int height, SurfaceFormat format)
        {
            return new RenderTarget2D(
                _graphics,
                width,
                height,
                false,
                format,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents);
        }

        private sealed class PooledRenderTarget
        {
            public RenderTarget2D RenderTarget { get; }
            public bool IsActive { get; set; }

            public PooledRenderTarget(RenderTarget2D renderTarget)
            {
                RenderTarget = renderTarget;
                IsActive = true;
            }

            public bool TryRent(int width, int height, SurfaceFormat format, out RenderTarget2D renderTarget)
            {
                if (!IsActive &&
                    RenderTarget.Width == width &&
                    RenderTarget.Height == height &&
                    RenderTarget.Format == format)
                {
                    IsActive = true;
                    renderTarget = RenderTarget;
                    return true;
                }

                renderTarget = null;
                return false;
            }
        }
    }
}
