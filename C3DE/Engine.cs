using C3DE.Inputs;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection;

namespace C3DE
{
    public enum GamePlatform
    {
        Unknown = 0, Windows, Desktop, UWP, Android
    }

    /// <summary>
    /// The starting point of the engine. Managers, registry objects, etc. are initialized here.
    /// </summary>
    public class Engine : Game
    {
        private bool _autoDetectResolution;
        private bool _requestFullscreen;
        protected GraphicsDeviceManager _graphicsDeviceManager;
        protected BaseRenderer renderer;
        private BaseRenderer _nextRenderer;
        protected SceneManager _sceneManager;
        protected bool _initialized;
        private int _totalFrames;
        private float _elapsedTime;
        private int _FPS = 0;

        public static GamePlatform Platform
        {
            get
            {
#if WINDOWS
                return GamePlatform.Windows;
#elif DESKTOP
                return GamePlatform.Desktop;
#elif NETFX_CORE
                return GamePlatform.UWP;
#elif ANDROID
                return GamePlatform.Android;
#else
                return GamePlatform.Unknown;
#endif
            }
        }

        public BaseRenderer Renderer
        {
            get => renderer;
            set => _nextRenderer = value;
        }

        public float FPS => _FPS;

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
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
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
            Application.GraphicsDeviceManager = _graphicsDeviceManager;
            Application.SceneManager = _sceneManager;

#if WINDOWS || DESKTOP
            _autoDetectResolution = width == 0 || height == 0;

            if (!_autoDetectResolution)
            {
                _graphicsDeviceManager.PreferredBackBufferWidth = width;
                _graphicsDeviceManager.PreferredBackBufferHeight = height;
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

#if ANDROID
            _graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft;
#endif

#if DESKTOP
            _graphicsDeviceManager.PreferredBackBufferWidth = Screen.Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = Screen.Height;
            _graphicsDeviceManager.ApplyChanges();
#endif

            GraphicsDevice.PresentationParameters.MultiSampleCount = 4;

            if (_autoDetectResolution)
                Screen.SetBestResolution(_requestFullscreen);

            renderer._graphicsDevice = GraphicsDevice;
            renderer.Initialize(Content);
            RendererChanged?.Invoke(renderer);

            Input.Keys = new KeyboardComponent(this);
            Input.Mouse = new MouseComponent(this);
            Input.Gamepad = new GamepadComponent(this);
            Input.Touch = new TouchComponent(this);

            Components.Add(new Time(this));
            Components.Add(Input.Keys);
            Components.Add(Input.Mouse);
            Components.Add(Input.Gamepad);
            Components.Add(Input.Touch);

            _graphicsDeviceManager.PreparingDeviceSettings += OnResize;
            _initialized = true;

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_elapsedTime > 1000.0f)
            {
                _FPS = _totalFrames;
                _totalFrames = 0;
                _elapsedTime = 0;
            }

            base.Update(gameTime);
            _sceneManager.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            _totalFrames++;
            GraphicsDevice.Clear(Color.Black);

            renderer.Render(_sceneManager.ActiveScene);

            base.Draw(gameTime);
        }

        protected override void EndDraw()
        {
            if (Screen.LockCursor)
                Mouse.SetPosition(Screen.WidthPerTwo, Screen.HeightPerTwo);

            base.EndDraw();

            if (_nextRenderer != null)
            {
                renderer?.Dispose();

                renderer = _nextRenderer;
                renderer.Initialize(Content);
                _nextRenderer = null;

                RendererChanged?.Invoke(renderer);
            }
        }
    }
}
