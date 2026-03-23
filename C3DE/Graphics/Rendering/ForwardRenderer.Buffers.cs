using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    public partial class ForwardRenderer
    {
        public override RenderTarget2D GetDepthBuffer()
        {
            _depthPass.Enabled = true;
            return TryGetCombinedBuffer(useDepthBuffer: true);
        }

        public override RenderTarget2D GetNormalBuffer()
        {
            _normalPass.Enabled = true;
            return TryGetCombinedBuffer(useDepthBuffer: false);
        }

        private RenderTarget2D TryGetCombinedBuffer(bool useDepthBuffer)
        {
            if (UseMRT && _depthPass.Enabled && _normalPass.Enabled)
            {
                _depthNormalPass.Enabled = true;
                return useDepthBuffer ? _depthNormalPass.DepthBuffer : _depthNormalPass.NormalBuffer;
            }

            _depthNormalPass.Enabled = false;
            return useDepthBuffer ? _depthPass.RenderTarget : _normalPass.RenderTarget;
        }
    }
}
