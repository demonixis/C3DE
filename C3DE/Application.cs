using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE
{
    public class Application
    {
        internal static Game Game { get; set; }
        public static ContentManager Content { get; set; }
        public static GraphicsDevice GraphicsDevice { get; set; }

        public static void TargetFrameRate(long frameRate)
        {
            Game.TargetElapsedTime = new TimeSpan(10000000L / frameRate);
        }

        public static void Quit()
        {
            Game.Exit();
        }
    }
}
