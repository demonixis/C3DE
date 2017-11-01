using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using C3DE.Coroutines;

namespace C3DE
{
    /// <summary>
    /// Static class Application that contains global objects used into the engine.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Gets or sets the Game instance.
        /// </summary>
        public static Engine Engine { get; set; }

        /// <summary>
        /// Gets or sets the content manager.
        /// </summary>
        public static ContentManager Content { get; set; }

        /// <summary>
        /// Gets or sets the graphics device.
        /// </summary>
        public static GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// Gets or sets the graphics device manager.
        /// </summary>
        public static GraphicsDeviceManager GraphicsDeviceManager { get; set; }

        /// <summary>
        /// Gets or sets the scene manager.
        /// </summary>
        public static SceneManager SceneManager { get; set; }

        /// <summary>
        /// Change the targeted frame rate (default is 60 fps). 
        /// </summary>
        /// <param name="frameRate"></param>
        public static void TargetFrameRate(long frameRate)
        {
            Engine.TargetElapsedTime = new TimeSpan(10000000L / frameRate);
        }

        internal static CoroutineManager CoroutineManager { get; set; }

        /// <summary>
        /// Quit the application.
        /// </summary>
        public static void Quit()
        {
#if !NETFX_CORE && !DESKTOP
            Engine.Exit();
#endif
        }
    }
}
