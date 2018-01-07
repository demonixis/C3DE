using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Graphics.PostProcessing
{
    public class TemporaryRenderTarget
    {
        public RenderTarget2D RenderTarget { get; }
        public bool IsActive { get; set; }

        public TemporaryRenderTarget(int width, int height, bool isActive)
        {
            var pp = Application.GraphicsDevice.PresentationParameters;
            RenderTarget = new RenderTarget2D(Application.GraphicsDevice, width, height, false, pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount, pp.RenderTargetUsage);
            IsActive = isActive;
        }

        public bool CanBeUsed(int width, int height)
        {
            if (IsActive)
                return false;

            return RenderTarget.Width == width && RenderTarget.Height == height;
        }
    }

    public static class RenderTexture
    {
        private static List<TemporaryRenderTarget> TemporaryRenderTargets = new List<TemporaryRenderTarget>();

        public static RenderTarget2D GetTemporary(int width, int height)
        {
            foreach (var target in TemporaryRenderTargets)
            {
                if (target.CanBeUsed(width, height))
                {
                    target.IsActive = true;
                    return target.RenderTarget;
                }
            }

            var tempTarget = new TemporaryRenderTarget(width, height, true);
            TemporaryRenderTargets.Add(tempTarget);

            return tempTarget.RenderTarget;
        }

        public static void ReleaseTemporary(RenderTarget2D renderTarget)
        {
            foreach (var target in TemporaryRenderTargets)
                if (target.RenderTarget == renderTarget)
                    target.IsActive = false;
        }

        public static void ReleaseAll()
        {
            foreach (var target in TemporaryRenderTargets)
                target.IsActive = false;
        }
    }
}
