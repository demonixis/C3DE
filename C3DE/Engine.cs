using C3DE.Inputs;
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
        private bool _started;
        private IRenderer _rendererToChange;
        private bool _needRendererChange;

        protected GraphicsDeviceManager graphics;
        protected IRenderer renderer;
        protected SceneManager sceneManager;
        protected bool initialized;

        public IRenderer Renderer
        {
            get { return renderer; }
            set 
            {
                _rendererToChange = value;
                _needRendererChange = true;
            }
        }

        public Engine(string title = "C3DE", int width = 1024, int height = 600)
            : base()
        {
            graphics = new GraphicsDeviceManager(this);

#if !ANDROID && !WINDOWS_PHONE
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
#endif
            Window.Title = title;
            Content.RootDirectory = "Content";
            sceneManager = new SceneManager();
            initialized = false;

            Application.Content = Content;
            Application.Game = this;
            Application.GraphicsDevice = GraphicsDevice;
            Application.GraphicsDeviceManager = graphics;
            Application.SceneManager = sceneManager;

            Screen.Setup(width, height, false, true);

            graphics.PreparingDeviceSettings += OnResize;

            _needRendererChange = false;
            _started = false;
        }

        private void OnResize(object sender, PreparingDeviceSettingsEventArgs e)
        {
            int width = e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth;
            int height = e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight;

            Screen.Setup(width, height, null, null);
        }

        protected void SetRenderer(IRenderer iRenderer)
        {
            renderer = iRenderer;

            if (initialized)
            {
#if ANDROID
                Screen.Setup (GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height, null, null);
#elif LINUX
                Screen.Setup(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, null, null);
                GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
#endif
                renderer.Initialize(Content);
            }
        }

        protected override void Initialize()
        {
            if (Application.GraphicsDevice == null)
                Application.GraphicsDevice = GraphicsDevice;

#if ANDROID
			Screen.Setup (GraphicsDevice.Adapter.CurrentDisplayMode.Width, GraphicsDevice.Adapter.CurrentDisplayMode.Height, null, null);
#elif LINUX
			Screen.Setup(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, null, null);
			GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
#endif
            
            if (renderer == null)
                renderer = new Renderer();

            renderer.Initialize(Content);

            Input.Keys = new KeyboardComponent(this);
            Input.Mouse = new MouseComponent(this);
            Input.Gamepad = new GamepadComponent(this);
            Input.Touch = new TouchComponent(this);

            Components.Add(new Time(this));
            Components.Add(Input.Keys);
            Components.Add(Input.Mouse);
            Components.Add(Input.Gamepad);
            Components.Add(Input.Touch);

            initialized = true;
           
            base.Initialize();
        }

        protected override void LoadContent()
        {
            sceneManager.Initialize();
        }

        protected override void BeginRun()
        {
            _started = true;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            sceneManager.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            renderer.render(sceneManager.ActiveScene, sceneManager.ActiveScene.MainCamera);
            base.Draw(gameTime);
        }

        protected override void EndDraw()
        {
            base.EndDraw();

            if (Screen.LockCursor)
                Mouse.SetPosition(Screen.WidthPerTwo, Screen.HeightPerTwo);

            if (_needRendererChange)
            {
                SetRenderer(_rendererToChange);
                _needRendererChange = false;
            }
        }
    }
}
