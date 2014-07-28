using Microsoft.Xna.Framework;

namespace C3DE
{
    public class Time : GameComponent
    {
        private static float __deltaTime = 1.0f;
        private static float __timeScale = 1.0f;

        public static float DeltaTime
        {
            get { return __deltaTime * __timeScale; }
        }

        public static float TimeScale
        {
            get { return __timeScale; }
            set { __timeScale = value; }
        }
        
        public Time(Game game)
            : base (game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            __deltaTime = gameTime.ElapsedGameTime.Milliseconds;
        }
    }
}
