using C3DE.Inputs;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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
        protected BaseRenderer renderer;
        private BaseRenderer m_nextRenderer;
        protected SceneManager _sceneManager;
        protected bool _initialized;
        protected GraphicsDevice m_GraphicsDevice;

        public BaseRenderer Renderer
        {
            get { return renderer; }
            set { m_nextRenderer = value; }
        }

        public event Action<BaseRenderer> RendererChanged = null;

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

            renderer = new ForwardRenderer(GraphicsDevice);

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

        public void InitializeEditor(GraphicsDevice device)
        {
            m_GraphicsDevice = device;
            Initialize();
        }

        public void UpdateEditor()
        {
            Update(null);
        }

        public void DrawEditor()
        {
            Draw(null);
        }

        public void EndDrawEditor()
        {
            EndDraw();
        }

        protected override void Initialize()
        {
            if (GraphicsDevice != null && m_GraphicsDevice == null)
                m_GraphicsDevice = GraphicsDevice;

            if (Application.GraphicsDevice == null)
                Application.GraphicsDevice = m_GraphicsDevice;

            if (_autoDetectResolution)
                Screen.SetBestResolution(_requestFullscreen);

            renderer.m_graphicsDevice = m_GraphicsDevice;
            renderer.Initialize(Content);
            RendererChanged?.Invoke(renderer);

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
            m_GraphicsDevice.Clear(Color.Black);
            renderer.Render(_sceneManager.ActiveScene);
            base.Draw(gameTime);
        }

        protected override void EndDraw()
        {
            if (Screen.LockCursor)
                Mouse.SetPosition(Screen.WidthPerTwo, Screen.HeightPerTwo);


            base.EndDraw();

            if (m_nextRenderer != null)
            {
                renderer?.Dispose();

                renderer = m_nextRenderer;
                renderer.Initialize(Content);
                m_nextRenderer = null;

                RendererChanged?.Invoke(renderer);
            }
        }
    }
}
