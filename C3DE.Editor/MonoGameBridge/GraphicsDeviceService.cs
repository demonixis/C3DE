using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Editor.MonoGameBridge
{
    public class GraphicsDeviceService : IGraphicsDeviceService
    {
        private GraphicsDevice _device;
        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        public GraphicsDevice GraphicsDevice
        {
            get { return _device; }
        }

        public GraphicsDeviceService(GraphicsDevice device)
        {
            _device = device;
        }
    }
}