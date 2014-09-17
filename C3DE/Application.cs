using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

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
        public static Game Game { get; set; }

        /// <summary>
        /// Gets or sets the content manager.
        /// </summary>
        public static ContentManager Content { get; set; }

        /// <summary>
        /// Gets or sets the graphics device.
        /// </summary>
        public static GraphicsDevice GraphicsDevice { get; set; }

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
            Game.TargetElapsedTime = new TimeSpan(10000000L / frameRate);
        }

        /// <summary>
        /// Load a scene by its name.
        /// </summary>
        /// <param name="name">The scene's name to load.</param>
        public static void LoadLevel(string name)
        {
            SceneManager.LoadLevel(name);
        }

        /// <summary>
        /// Load a scene by its index.
        /// </summary>
        /// <param name="index">The scene's index to load.</param>
        public static void LoadLevel(int index)
        {
            SceneManager.LoadLevel(index);
        }

        /// <summary>
        /// Quit the application.
        /// </summary>
        public static void Quit()
        {
            Game.Exit();
        }
    }
}
