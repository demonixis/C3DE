#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using C3DE;

#endregion

/* 
 * Source Code orignally written by Nick Gravelyn
 * Original blog post: http://blogs.msdn.com/b/nicgrave/archive/2010/07/25/rendering-with-xna-framework-4-0-inside-of-a-wpf-application.aspx
 * 
 * Modified and uploaded with permission.
 */

namespace MerjTek.WpfIntegration
{
    /// <summary>
    /// Helper class responsible for creating and managing the GraphicsDevice.
    /// All GraphicsDeviceControl instances share the same GraphicsDeviceService,
    /// so even though there can be many controls, there will only ever be a 
    /// single underlying GraphicsDevice. This implements the standard 
    /// IGraphicsDeviceService interface, which provides notification events for 
    /// when the device is reset or disposed.
    /// </summary>
    public class GraphicsDeviceService : IGraphicsDeviceService
    {
        #region Object Definitions

        // Singleton device service instance.
        private static GraphicsDeviceService singletonInstance;

        // Keep track of how many controls are sharing the singletonInstance.
        private static int referenceCount;

        #endregion
        #region Public Properties

        /// <summary>
        /// Gets the single instance of the service class for the application.
        /// </summary>
        public static GraphicsDeviceService Instance
        {
            get
            {
                if (null == singletonInstance)
                    singletonInstance = new GraphicsDeviceService();
                return singletonInstance;
            }
        }

        /// <summary>
        /// Gets the current graphics evice manager.
        /// This was added so that the control would work with MonoGame
        /// </summary>
        public GraphicsDeviceManager graphics { get; private set; }

        /// <summary>
        /// Gets the current graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Engine Engine { get; private set; }

        // IGraphicsDeviceService events.
        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        #endregion

        /// <summary>
        /// Constructor is private, because this is a singleton class:
        /// client controls should use the public AddRef method instead.
        /// </summary>
        GraphicsDeviceService() { }

        #region AddRef

        /// <summary>
        /// Gets a reference to the singleton instance.
        /// </summary>
        public static GraphicsDeviceService AddRef(IntPtr windowHandle)
        {
            // Increment the "how many controls sharing the device" reference count.
            if (1 == Interlocked.Increment(ref referenceCount))
            {
                // If this is the first control to start using the device...

                // Create the GraphicsDevice for the service
                var game = new Engine();

                PresentationParameters parameters = new PresentationParameters();

                // Required for control to work with MonoGame.
                // If this line is removed, the GraphicsDevice creation below will fail.
                //Instance.graphics = new GraphicsDeviceManager(game);

                // since we're using render targets anyway, the 
                // backbuffer size is somewhat irrelevant
                parameters.BackBufferWidth = 480;
                parameters.BackBufferHeight = 320;
                parameters.BackBufferFormat = SurfaceFormat.Color;
                parameters.DeviceWindowHandle = windowHandle;
                parameters.DepthStencilFormat = DepthFormat.Depth24Stencil8;
                parameters.IsFullScreen = false;

                Instance.GraphicsDevice = new GraphicsDevice(
                    GraphicsAdapter.DefaultAdapter,
                    GraphicsProfile.HiDef,
                    parameters);

                Instance.Engine = game;

                if (Instance.DeviceCreated != null)
                    Instance.DeviceCreated(Instance, EventArgs.Empty);
            }

            return singletonInstance;
        }

        #endregion
        #region Release

        /// <summary>
        /// Releases a reference to the singleton instance.
        /// </summary>
        public void Release()
        {
            // Decrement the "how many controls sharing the device" reference count
            if (0 == Interlocked.Decrement(ref referenceCount))
            {
                // If this is the last control to finish using the device, we should dispose the singleton instance.
                if (null != DeviceDisposing)
                    DeviceDisposing(this, EventArgs.Empty);

                GraphicsDevice.Dispose();
                GraphicsDevice = null;
            }
        }

        #endregion
    }
}
