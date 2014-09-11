namespace C3DE
{
    public class Screen
    {
        public static int Width { get; internal set; }
        public static int Height { get; internal set; }

        public static int WidthPerTwo { get; internal set; }
        public static int HeightPerTwo { get; internal set; }

        public static bool LockCursor { get; set; }

        public static bool ShowCursor
        {
            get { return Application.Game.IsMouseVisible; }
            set { Application.Game.IsMouseVisible = value; }
        }

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
