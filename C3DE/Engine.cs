using C3DE.Inputs;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C3DE
{
    /// <summary>
    /// The starting point of the engine. Managers, registry objects, etc. are initialized here.
    /// </summary>
    public class Engine : Game
    {
        private bool _autoDetectResolution;
        private bool _requestFullscreen;
        protected GraphicsDeviceManager _graphics;
        protected Renderer renderer;
        private Renderer m_nextRenderer;
        protected SceneManager _sceneManager;
        protected bool _initialized;

        public Renderer Renderer
        {
            get { return renderer; }
            set { m_nextRenderer = value; }
        }

        public Engine()
            : this("C3DE Game")
        {
        }

        /// <summary>
        /// Creates the game by initializing graphics, input and other managers.
        /// The default configuration use the best resolution and toggle in fullscreen mode.
        /// </summary>
        /// <param name="title">The title of the game.</param>
        /// <param name="width">Desired screen width.</param>
        /// <param name="height">Desired screen height.</param>
        /// <param name="fullscreen">Sets to true to use the fullscreen mode.</param>
        public Engine(string title = "C3DE Game", int width = 0, int height = 0, bool fullscreen = false)
            : base()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            _sceneManager = new SceneManager();
            _initialized = false;
            _autoDetectResolution = false;
            _requestFullscreen = false;

            Window.Title = title;
            Content.RootDirectory = "Content";

            Application.Content = Content;
            Application.Engine = this;
            Application.GraphicsDevice = GraphicsDevice;
            Application.GraphicsDeviceManager = _graphics;
            Application.SceneManager = _sceneManager;

#if WINDOWS || DESKTOP
            _autoDetectResolution = width == 0 || height == 0;

            if (!_autoDetectResolution)
            {
                _graphics.PreferredBackBufferWidth = width;
                _graphics.PreferredBackBufferHeight = height;
            }
#else
            if (width == 0 || height == 0)
            {
                width = Window.ClientBounds.Width;
                height = Window.ClientBounds.Height;
            }
#endif

            Screen.Setup(width, height, false, true);
            Screen.SetVirtualResolution(width, height, false);
        }

        private void OnResize(object sender, PreparingDeviceSettingsEventArgs e)
        {
            var pp = e.GraphicsDeviceInformation.PresentationParameters;
            var width = pp.BackBufferWidth;
            var height = pp.BackBufferHeight;
            Screen.Setup(width, height, null, null);

            if (UI.GUI.Scale != Vector2.One)
                UI.GUI.Scale = Screen.GetScale();

            renderer.Dirty = true;
        }

        protected override void Initialize()
        {
            if (Application.GraphicsDevice == null)
                Application.GraphicsDevice = GraphicsDevice;

            if (_autoDetectResolution)
                Screen.SetBestResolution(_requestFullscreen);

            if (renderer == null)
            {
                renderer = new ForwardRenderer(GraphicsDevice);
                renderer.Initialize(Content);
            }

            Serializer.AddTypes(typeof(Engine));

            Input.Keys = new KeyboardComponent(this);
            Input.Mouse = new MouseComponent(this);
            Input.Gamepad = new GamepadComponent(this);
            Input.Touch = new TouchComponent(this);

            Components.Add(new Time(this));
            Components.Add(Input.Keys);
            Components.Add(Input.Mouse);
            Components.Add(Input.Gamepad);
            Components.Add(Input.Touch);

            _graphics.PreparingDeviceSettings += OnResize;
            _initialized = true;

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _sceneManager.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            renderer.Render(_sceneManager.ActiveScene);
            base.Draw(gameTime);
        }

        protected override void EndDraw()
        {
            if (Screen.LockCursor)
                Mouse.SetPosition(Screen.WidthPerTwo, Screen.HeightPerTwo);

            if (m_nextRenderer != null)
            {
                renderer?.Dispose();

                renderer = m_nextRenderer;
                renderer.Initialize(Content);
                m_nextRenderer = null;
            }

            base.EndDraw();
        }
    }
}
