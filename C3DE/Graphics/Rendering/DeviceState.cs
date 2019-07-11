using System;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    public sealed class DeviceState : IDisposable
    {
        private readonly GraphicsDevice _device;
        private readonly BlendState _blendState;
        private readonly DepthStencilState _depthStencilState;
        private readonly RasterizerState _rasterizerState;
        private readonly SamplerState _samplerState;

        public DeviceState(GraphicsDevice device, BlendState blendState, DepthStencilState depthStencilState, RasterizerState rasterizerState, SamplerState samplerState)
        {
            _device = device;
            _blendState = device.BlendState;
            _depthStencilState = device.DepthStencilState;
            _rasterizerState = device.RasterizerState;
            _samplerState = device.SamplerStates[0];

            device.BlendState = blendState;
            device.DepthStencilState = depthStencilState;
            device.RasterizerState = rasterizerState;
            device.SamplerStates[0] = samplerState;
        }

        public void Dispose()
        {
            _device.BlendState = _blendState;
            _device.DepthStencilState = _depthStencilState;
            _device.RasterizerState = _rasterizerState;
            _device.SamplerStates[0] = _samplerState;
        }
    }
}
