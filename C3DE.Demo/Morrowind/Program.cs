using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Morrowind
{
    class Program
    {
        public const bool Fullscreen = false;
        public const int ScreenWidth = 1440;
        public const int ScreenHeight = 900;
        public const int UIWidth = 1280;
        public const int UIHeight = 720;

        public static string[] BlueSkybox =
        {
            "Textures/Skybox/bluesky/px",
            "Textures/Skybox/bluesky/nx",
            "Textures/Skybox/bluesky/py",
            "Textures/Skybox/bluesky/ny",
            "Textures/Skybox/bluesky/pz",
            "Textures/Skybox/bluesky/nz"
        };

        public static string[] StarsSkybox =
        {
            "Textures/Skybox/starfield/px",
            "Textures/Skybox/starfield/nx",
            "Textures/Skybox/starfield/py",
            "Textures/Skybox/starfield/ny",
            "Textures/Skybox/starfield/pz",
            "Textures/Skybox/starfield/nz"
        };

        public static GUISkin CreateSkin(ContentManager content, bool customSkin = true)
        {
            GUISkin skin = new GUISkin("Font/SegoeUILight");
            skin.LoadContent(Application.Content);

            if (customSkin)
            {
                skin.Box = content.Load<Texture2D>("Textures/UI/grey_panel");
                skin.Buttons[0] = content.Load<Texture2D>("Textures/UI/grey_button00");
                skin.Buttons[1] = content.Load<Texture2D>("Textures/UI/grey_button01");
                skin.Buttons[2] = content.Load<Texture2D>("Textures/UI/grey_button02");
                skin.Checkbox[0] = content.Load<Texture2D>("Textures/UI/grey_box");
                skin.Checkbox[1] = content.Load<Texture2D>("Textures/UI/grey_checkmarkWhite");
                skin.Checkbox[2] = content.Load<Texture2D>("Textures/UI/grey_checkmarkGrey");
                skin.Sliders[0] = content.Load<Texture2D>("Textures/UI/grey_button02");
                skin.Sliders[1] = content.Load<Texture2D>("Textures/UI/grey_button00");
                skin.TextColor = Color.Black;
            }

            return skin;
        }

        public static void InitializeGame()
        {
            Application.SceneManager.AddRange(new Scene[]
            {
                new MorrowindGameScene()
            }, 0);

            Application.SceneManager.LoadLevel(0);
            Screen.Fullscreen = Fullscreen;
            //Screen.SetVirtualResolution(UIWidth, UIHeight, true);
        }

        static void Main(string[] args)
        {
            using (var game = new C3DE.Engine("C3DE Morrowind", ScreenWidth, ScreenHeight))
            {
                InitializeGame();
                game.Run();
            }
        }
    }
}
