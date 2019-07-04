using static C3DE.Demo.DemoGame;

namespace C3DE.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new Engine("C3DE Samples", ScreenWidth, ScreenHeight))
            {
                InitializeGame();
                game.Run();
            }
        }
    }
}
