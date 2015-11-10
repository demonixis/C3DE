using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE
{
    /// <summary>
    /// A static class to gets informations about the screen.
    /// </summary>
    public class Screen
    {
        public static event EventHandler<EventArgs> ScreenSizeChanged = null;

        /// <summary>
        /// Gets the rectangle that represent the screen size
        /// </summary>
        public static Rectangle ScreenRect { get; internal set; }

        public static bool Fullscreen
        {
            get { return Application.GraphicsDeviceManager.IsFullScreen; }
            set
            {
                if (Application.GraphicsDeviceManager.IsFullScreen != value)
                    ToggleFullscreen();
            }
        }

        /// <summary>
        /// Gets the height of the screen (this value is cached so you can use it safely).
        /// </summary>
        public static int Width
        {
            get { return ScreenRect.Width; }
        }

        /// <summary>
        /// Gets the height of the screen (this value is cached so you can use it safely).
        /// </summary>
        public static int Height
        {
            get { return ScreenRect.Height; }
        }

        /// <summary>
        /// Gets the half-width of the screen (this value is cached so you can use it safely).
        /// </summary>
        public static int WidthPerTwo { get; internal set; }

        /// <summary>
        /// Gets the half-height of the screen (this value is cached so you can use it safely).
        /// </summary>
        public static int HeightPerTwo { get; internal set; }

        /// <summary>
        /// Gets the virtual screen rectangle.
        /// </summary>
        public static Rectangle VirtualScreenRect { get; internal set; }

        /// <summary>
        /// The virtual width of the screen.
        /// </summary>
        public static int VirtualWidth
        {
            get { return VirtualScreenRect.Width; }
        }

        /// <summary>
        /// The virtual height of the screen.
        /// </summary>
        public static int VirtualHeight
        {
            get { return VirtualScreenRect.Height; }
        }

        /// <summary>
        /// The virtual width divided per two.
        /// </summary>
        public static int VirtualWidthPerTwo { get; internal set; }

        /// <summary>
        /// The virtual height divided per two.
        /// </summary>
        public static int VirtualHeightPerTwo { get; internal set; }

        /// <summary>
        /// Lock or not the mouse cursor.
        /// </summary>
        public static bool LockCursor { get; set; }

        /// <summary>
        /// Show or hide the mouse cursor.
        /// </summary>
        public static bool ShowCursor
        {
            get { return Application.Engine.IsMouseVisible; }
            set { Application.Engine.IsMouseVisible = value; }
        }

        /// <summary>
        /// Setup the helper.
        /// </summary>
        /// <param name="width">The width of the screen.</param>
        /// <param name="height">The height of the screen.</param>
        /// <param name="lockCursor">Indicates whether the cursor is locked.</param>
        /// <param name="showCursor">Indicates whether the cursor is visible.</param>
        public static void Setup(int width, int height, bool? lockCursor, bool? showCursor)
        {
            if (width == 0)
                width = 800;
            if (height == 0)
                height = 480;

            ScreenRect = new Rectangle(0, 0, width, height);

            WidthPerTwo = width >> 1;
            HeightPerTwo = height >> 1;

            if (VirtualWidth == 0 || VirtualHeight == 0)
                SetVirtualResolution(width, height);

            if (lockCursor.HasValue)
                LockCursor = lockCursor.Value;

            if (showCursor.HasValue)
                ShowCursor = showCursor.Value;

            if (ScreenSizeChanged != null)
                ScreenSizeChanged(Application.Engine, EventArgs.Empty);
        }

        public static void SetVirtualResolution(int width, int height, bool applyToGUI = true)
        {
            VirtualScreenRect = new Rectangle(0, 0, width, height);
            VirtualWidthPerTwo = width >> 1;
            VirtualHeightPerTwo = height >> 1;

            if (applyToGUI)
                GUI.Scale = GetScale();
        }

        /// <summary>
        /// Get the scaled X coordinate relative to the reference width
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float GetScaleX(float value)
        {
            return (((float)Width * value) / (float)VirtualWidth);
        }

        /// <summary>
        /// Get the scaled Y coordinate relative to the reference height
        /// </summary>
        /// <param name="value">The default Y coordinate used with the reference height</param>
        /// <returns>A scaled Y coordinate</returns>
        public static float GetScaleY(float value)
        {
            return (((float)Height * value) / (float)VirtualHeight);
        }

        /// <summary>
        /// Gets the scale relative to the reference width and height
        /// </summary>
        /// <returns>The scale difference between the current resolution and the reference resolution of the screen</returns>
        public static Vector2 GetScale()
        {
            return new Vector2(
                (float)((float)Width / (float)VirtualWidth),
                (float)((float)Height / (float)VirtualHeight));
        }

        /// <summary>
        /// Determines the max resolution.
        /// </summary>
        /// <param name="fullscreen">If set to <c>true</c> fullscreen.</param>
        public static void SetBestResolution(bool fullscreen)
        {
            var modes = Application.GraphicsDevice.Adapter.SupportedDisplayModes;
            var width = 800;
            var height = 480;

            foreach (DisplayMode mode in modes)
            {
                width = (mode.Width > width) ? mode.Width : width;
                height = (mode.Height > height) ? mode.Height : height;
            }

            Application.GraphicsDeviceManager.PreferredBackBufferWidth = width;
            Application.GraphicsDeviceManager.PreferredBackBufferHeight = height;
            Application.GraphicsDeviceManager.ApplyChanges();

            Setup(width, height, null, null);

            if (Application.GraphicsDeviceManager.IsFullScreen && fullscreen)
                Application.GraphicsDeviceManager.ToggleFullScreen();
        }

        /// <summary>
        /// Toggles fullscreen/windowed mode.
        /// </summary>
        public static void ToggleFullscreen()
        {
            Application.GraphicsDeviceManager.ToggleFullScreen();
        }
    }
}
