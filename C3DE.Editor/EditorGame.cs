using C3DE.UI;

namespace C3DE.Editor
{
    public class EditorGame : Engine
    {
        public EditorGame()
            : base("C3DE Editor", 1440, 900, false)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();

            GUI.Skin = new GUISkin("Font/Menu");
            GUI.Skin.LoadContent(Content);

            Application.SceneManager.Add(new EditorScene());
            Application.SceneManager.LoadLevel(0);
        }

#if !ANDROID && !NETFX_CORE
        static void Main(string[] args)
        {
            using (var game = new EditorGame())
                game.Run();
        }
#endif
    }
}
