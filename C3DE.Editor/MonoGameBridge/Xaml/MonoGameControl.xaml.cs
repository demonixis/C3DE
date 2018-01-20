#region Using Statements

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Graphics;

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
    /// Interaction logic for MonoGameControl.xaml
    /// </summary>
    public partial class MonoGameControl : UserControl
    {
        private GraphicsDeviceService graphicsService;
        private RenderTargetImageSource imageSource;

        /// <summary>
        /// Gets the GraphicsDevice behind the control.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsService.GraphicsDevice; }
        }

        /// <summary>
        /// Invoked when the control is loaded.
        /// </summary>
        public event EventHandler<GraphicsDeviceEventArgs> ControlLoaded;

        /// <summary>
        /// Invoked when the control needs to be redrawn.
        /// </summary>
        public event EventHandler<GraphicsDeviceEventArgs> Render;

        public MonoGameControl()
        {
            InitializeComponent();
        }

        ~MonoGameControl()
        {
            imageSource.Dispose();

            // release on finalizer to clean up the graphics device
            if (graphicsService != null)
                graphicsService.Release();
        }

        void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // if we're not in design mode, initialize the graphics device
            if (DesignerProperties.GetIsInDesignMode(this) == false && graphicsService == null)
            {
                // add a reference to the graphics device
                graphicsService = GraphicsDeviceService.AddRef((PresentationSource.FromVisual(this) as HwndSource).Handle);

                // create the image source
                imageSource = new RenderTargetImageSource(GraphicsDevice, (int)ActualWidth, (int)ActualHeight);
                rootImage.Source = imageSource.WriteableBitmap;

                // hook the rendering event
                CompositionTarget.Rendering += CompositionTarget_Rendering;

                // Invoke the ControlLoaded event
                if (ControlLoaded != null)
                    ControlLoaded(this, new GraphicsDeviceEventArgs(graphicsService.GraphicsDevice));
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            // if we're not in design mode, recreate the image source for the new size
            if (DesignerProperties.GetIsInDesignMode(this) == false && graphicsService != null)
            {
                // recreate the image source
                imageSource.Dispose();
                imageSource = new RenderTargetImageSource(GraphicsDevice, (int)ActualWidth, (int)ActualHeight);
                rootImage.Source = imageSource.WriteableBitmap;
            }

            base.OnRenderSizeChanged(sizeInfo);
        }

        /// <summary>
        /// Draws the control and allows subclasses to override the default behavior of delegating the rendering.
        /// </summary>
        protected virtual void RenderControl()
        {
            // Set the background color
            int r = (Background as SolidColorBrush).Color.R;
            int g = (Background as SolidColorBrush).Color.G;
            int b = (Background as SolidColorBrush).Color.B;
            int a = (Background as SolidColorBrush).Color.A;
            GraphicsDevice.Clear(new Microsoft.Xna.Framework.Color(r, g, b, a));

            // invoke the draw delegate so someone will draw something pretty
            if (Render != null)
                Render(this, new GraphicsDeviceEventArgs(GraphicsDevice));
        }

        /// <summary>
        /// Draws to the RenderTarget and commits it to the ImageSource
        /// </summary>
        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // set the image source render target
            GraphicsDevice.SetRenderTarget(imageSource.RenderTarget);

            // allow the control to draw
            RenderControl();

            // unset the render target
            GraphicsDevice.SetRenderTarget(null);

            // commit the changes to the image source
            imageSource.Commit();
        }
    }
}
