namespace C3DE.Demo
{
    public static class Demo
    {
        public static int ScreenWidth = 1440;
        public static int ScreenHeight = 900;

        public static string[] BlueSkybox = new string[6] 
        {
            "Textures/Skybox/bluesky/px",   
            "Textures/Skybox/bluesky/nx",
            "Textures/Skybox/bluesky/py",
            "Textures/Skybox/bluesky/ny",
            "Textures/Skybox/bluesky/pz",
            "Textures/Skybox/bluesky/nz"
        };

        public static string[] StarsSkybox = new string[] 
        {
            "Textures/Skybox/starfield/px",   
            "Textures/Skybox/starfield/nx",
            "Textures/Skybox/starfield/py",
            "Textures/Skybox/starfield/ny",
            "Textures/Skybox/starfield/pz",
            "Textures/Skybox/starfield/nz"
        };

        // Entry point.
        static void Main(string[] args)
        {
            using (var game = new TerrainDemo())
                game.Run();
        }
    }
}
