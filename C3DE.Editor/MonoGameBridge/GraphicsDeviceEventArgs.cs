#region Using Statements

using System;
using Microsoft.Xna.Framework.Graphics;

#endregion

/* 
 * Source Code orignally written by Nick Gravelyn
 * Original blog post: http://blogs.msdn.com/b/nicgrave/archive/2010/07/25/rendering-with-xna-framework-4-0-inside-of-a-wpf-application.aspx
 * 
 * This file taken from: http://blogs.msdn.com/b/nicgrave/archive/2011/03/25/wpf-hosting-for-xna-game-studio-4-0.aspx
 * 
 * Modified and uploaded with permission.
 */


namespace MerjTek.WpfIntegration
{
    /// <summary>
    /// Arguments used for GraphicsDevice related events.
    /// </summary>
    public class GraphicsDeviceEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Initializes a new GraphicsDeviceEventArgs.
        /// </summary>
        /// <param name="device">The GraphicsDevice associated with the event.</param>
        public GraphicsDeviceEventArgs(GraphicsDevice device)
        {
            GraphicsDevice = device;
        }
    }
}
