#region Using Statements

using System;
using System.Windows;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;

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
    /// A wrapper for a RenderTarget2D and WriteableBitmap 
    /// that handles taking the XNA rendering and moving it 
    /// into the WriteableBitmap which is consumed as the
    /// ImageSource for an Image control.
    /// </summary>
    public class RenderTargetImageSource : IDisposable
    {
        #region Object Definitions

        // the render target we draw to
        private RenderTarget2D renderTarget;

        // a WriteableBitmap we copy the pixels into for display into the Image
        private WriteableBitmap writeableBitmap;

        // a buffer array that gets the data from the render target
        private byte[] buffer;

        #endregion
        #region Public Properties

        /// <summary>
        /// Gets the render target used for this image source.
        /// </summary>
        public RenderTarget2D RenderTarget
        {
            get { return renderTarget; }
        }

        /// <summary>
        /// Gets the underlying WriteableBitmap that can 
        /// be bound as an ImageSource.
        /// </summary>
        public WriteableBitmap WriteableBitmap
        {
            get { return writeableBitmap; }
        }

        #endregion

        /// <summary>
        /// Creates a new RenderTargetImageSource.
        /// </summary>
        /// <param name="graphics">The GraphicsDevice to use.</param>
        /// <param name="width">The width of the image source.</param>
        /// <param name="height">The height of the image source.</param>
        public RenderTargetImageSource(GraphicsDevice graphics, int width, int height)
        {
            // create the render target and buffer to hold the data
            renderTarget = new RenderTarget2D(graphics, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            buffer = new byte[width * height * 4];
            writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        }

        ~RenderTargetImageSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            renderTarget.Dispose();

            if (disposing)
                GC.SuppressFinalize(this);
        }

        #region Commit

        /// <summary>
        /// Commits the render target data into our underlying bitmap source.
        /// </summary>
        public void Commit()
        {
            // get the data from the render target
            renderTarget.GetData(buffer);

            // because the only 32 bit pixel format for WPF is 
            // BGRA but XNA is all RGBA, we have to swap the R 
            // and B bytes for each pixel
            for (int i = 0; i < buffer.Length - 2; i += 4)
            {
                byte r = buffer[i];
                buffer[i] = buffer[i + 2];
                buffer[i + 2] = r;
            }

            // write our pixels into the bitmap source
            writeableBitmap.Lock();
            Marshal.Copy(buffer, 0, writeableBitmap.BackBuffer, buffer.Length);
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, renderTarget.Width, renderTarget.Height));
            writeableBitmap.Unlock();
        }

        #endregion
    }
}
