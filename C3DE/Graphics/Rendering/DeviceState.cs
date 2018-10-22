using System;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Rendering
{
    internal sealed class DeviceState : IDisposable
    {
        private readonly GraphicsDevice m_Device;
        private readonly BlendState m_BlendState;
        private readonly DepthStencilState m_DepthStencilState;
        private readonly RasterizerState m_RasterizerState;
        private readonly SamplerState m_SamplerState;

        public DeviceState(GraphicsDevice device, BlendState blendState, DepthStencilState depthStencilState, RasterizerState rasterizerState, SamplerState samplerState)
        {
            m_Device = device;
            m_BlendState = device.BlendState;
            m_DepthStencilState = device.DepthStencilState;
            m_RasterizerState = device.RasterizerState;
            m_SamplerState = device.SamplerStates[0];

            device.BlendState = blendState;
            device.DepthStencilState = depthStencilState;
            device.RasterizerState = rasterizerState;
            device.SamplerStates[0] = samplerState;
        }

        public void Dispose()
        {
            m_Device.BlendState = this.m_BlendState;
            m_Device.DepthStencilState = this.m_DepthStencilState;
            m_Device.RasterizerState = this.m_RasterizerState;
            m_Device.SamplerStates[0] = this.m_SamplerState;
        }
    }
}
