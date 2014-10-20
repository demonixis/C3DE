namespace C3DE
{
    /// <summary>
    /// A static class to gets informations about the screen.
    /// </summary>
    public class Screen
    {
        /// <summary>
        /// Gets the height of the screen (this value is cached so you can use it safely).
        /// </summary>
        public static int Width { get; internal set; }

        /// <summary>
        /// Gets the height of the screen (this value is cached so you can use it safely).
        /// </summary>
        public static int Height { get; internal set; }

        /// <summary>
        /// Gets the half-width of the screen (this value is cached so you can use it safely).
        /// </summary>
        public static int WidthPerTwo { get; internal set; }

        /// <summary>
        /// Gets the half-height of the screen (this value is cached so you can use it safely).
        /// </summary>
        public static int HeightPerTwo { get; internal set; }

        /// <summary>
        /// Lock or not the mouse cursor.
        /// </summary>
        public static bool LockCursor { get; set; }

        /// <summary>
        /// Show or hide the mouse cursor.
        /// </summary>
        public static bool ShowCursor
        {
            get { return Application.Game.IsMouseVisible; }
            set { Application.Game.IsMouseVisible = value; }
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
            Width = width;
            Height = height;
            WidthPerTwo = width >> 1;
            HeightPerTwo = height >> 1;

            if (lockCursor.HasValue)
                LockCursor = lockCursor.Value;

            if (showCursor.HasValue)
                ShowCursor = showCursor.Value;
        }
    }
}
