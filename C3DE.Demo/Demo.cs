using C3DE.Materials;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Demo
{
    // Entry point.
    static class Demo
    {
        static void Main(string[] args)
        {
            using (var game = new GridDemo())
                game.Run();
        }
    }
}
