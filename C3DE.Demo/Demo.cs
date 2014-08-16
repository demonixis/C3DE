using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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

        public static GUISkin CreateSkin(ContentManager content)
        {
            GUISkin skin = new GUISkin();
            skin.Box = content.Load<Texture2D>("Textures/UI/grey_panel");
            skin.Buttons[0] = content.Load<Texture2D>("Textures/UI/grey_button00");
            skin.Buttons[1] = content.Load<Texture2D>("Textures/UI/grey_button01");
            skin.Buttons[2] = content.Load<Texture2D>("Textures/UI/grey_button02");
            skin.Checkbox[0] = content.Load<Texture2D>("Textures/UI/grey_box");
            skin.Checkbox[1] = content.Load<Texture2D>("Textures/UI/grey_checkmarkWhite");
            skin.Checkbox[2] = content.Load<Texture2D>("Textures/UI/grey_checkmarkGrey");
            skin.Font = content.Load<SpriteFont>("Font/SegoeUILight");
            skin.TextColor = Color.Black;
            return skin;
        }

        // Entry point.
        static void Main(string[] args)
        {
            using (var game = new TerrainDemo())
                game.Run();
        }
    }
}
