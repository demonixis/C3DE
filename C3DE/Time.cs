using Microsoft.Xna.Framework;

namespace C3DE
{
    /// <summary>
    /// A component responsible to manage time.
    /// </summary>
    public class Time : GameComponent
    {
        private static float __deltaTime = 0.0f;
        private static float __time = 0.0f;

        /// <summary>
        /// Gets the elapsed time since the last frame.
        /// </summary>
        public static float DeltaTime => __deltaTime * TimeScale / 1000.0f;

        public static float UnscaledDeltaTime => __deltaTime / 1000.0f;

        /// <summary>
        /// Gets the time scale.
        /// </summary>
        public static float TimeScale { get; set; } = 1.0f;

        public static float TotalTime => __time / 1000.0f;

        public Time(Game game = null)
            : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            __deltaTime = gameTime.ElapsedGameTime.Milliseconds;
            __time = (float)gameTime.TotalGameTime.TotalMilliseconds;
        }
    }
}
