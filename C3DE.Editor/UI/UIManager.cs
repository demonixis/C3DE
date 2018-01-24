using Gwen;
using Gwen.Control;
using Gwen.Platform;
using Gwen.Renderer.MonoGame;
using Gwen.Renderer.MonoGame.Input;
using Gwen.Skin;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Editor.UI
{
    public sealed class UIManager : DrawableGameComponent
    {
        private MonoGameInput m_Input;
        private MonoGameRenderer m_Renderer;
        private SkinBase m_Skin;
        private Canvas m_Canvas;

        public UIManager(Game game)
            : base(game)
        {
        }

        public void Initialize(GraphicsDeviceManager m_Graphics)
        {
            Platform.Init(new Gwen.Platform.MonoGame.MonoGamePlatform());
            Gwen.Loader.LoaderBase.Init(new Gwen.Loader.MonoGame.MonoGameAssetLoader(Game.Content));

            m_Renderer = new Gwen.Renderer.MonoGame.MonoGameRenderer(GraphicsDevice, Game.Content, Game.Content.Load<Effect>("GwenEffect"));
            m_Renderer.Resize(m_Graphics.PreferredBackBufferWidth, m_Graphics.PreferredBackBufferHeight);

            m_Skin = new Gwen.Skin.TexturedBase(m_Renderer, "Skins/DefaultSkin");
            m_Skin.DefaultFont = new Font(m_Renderer, "Arial", 11);
            m_Canvas = new Canvas(m_Skin);
            m_Input = new Gwen.Renderer.MonoGame.Input.MonoGameInput(Game);
            m_Input.Initialize(m_Canvas);

            m_Canvas.SetSize(m_Graphics.PreferredBackBufferWidth, m_Graphics.PreferredBackBufferHeight);
            m_Canvas.ShouldDrawBackground = true;
            m_Canvas.BackgroundColor = new Gwen.Color(255, 150, 170, 170);

            Game.Window.ClientSizeChanged += (e, i) => OnClientSizeChanged(m_Graphics);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            if (m_Canvas != null)
            {
                m_Canvas.Dispose();
                m_Canvas = null;
            }
            if (m_Skin != null)
            {
                m_Skin.Dispose();
                m_Skin = null;
            }
            if (m_Renderer != null)
            {
                m_Renderer.Dispose();
                m_Renderer = null;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            m_Input.ProcessMouseState();
            m_Input.ProcessKeyboardState();
            m_Input.ProcessTouchState();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            m_Canvas.RenderCanvas();
        }

        private void OnClientSizeChanged(GraphicsDeviceManager m_Graphics)
        {
            m_Renderer.Resize(m_Graphics.PreferredBackBufferWidth, m_Graphics.PreferredBackBufferHeight);
            m_Canvas.SetSize(m_Graphics.PreferredBackBufferWidth, m_Graphics.PreferredBackBufferHeight);
        }
    }
}
