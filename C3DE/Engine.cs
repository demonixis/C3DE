using C3DE.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE
{
    public class Application
    {
        internal static Game Game { get; set; }
        public static ContentManager Content { get; internal set; }
        public static GraphicsDevice GraphicsDevice { get; internal set; }
        
        public static void TargetFrameRate(long frameRate)
        {
            Game.TargetElapsedTime = new TimeSpan(10000000L / frameRate);
        }

        public static void Quit()
        {
            Game.Exit();
        }
    }

    public class Screen
    {
        public static int Width { get; internal set; }
        public static int Height { get; internal set; }

        public static int WidthPerTwo { get; internal set; }
        public static int HeightPerTwo { get; internal set; }

        public static bool LockCursor { get; set; }

        public static bool ShowCursor
        {
            get { return Application.Game.IsMouseVisible; }
            set { Application.Game.IsMouseVisible = value; }
        }

        internal static void Setup(int width, int height, bool? lockCursor, bool? showCursor)
        {
            Width = width;
            Height = height;
            WidthPerTwo = width >> 1;
            HeightPerTwo = height >> 1;

            if (lockCursor.HasValue)
                LockCursor = lockCursor.Value;

            if (showCursor.HasValue)
                ShowCursor = showCursor.Value;
        }
    }

    public class Input
    {
        public static KeyboardComponent Keys { get; internal set; }
        public static MouseComponent Mouse { get; internal set; }
        public static GamepadComponent Gamepad { get; internal set; }
    }

    public class Engine : Game
    {
        protected GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;
        protected Renderer renderer;
        protected Scene scene;

        public Engine(string title = "C3DE", int width = 1024, int height = 600)
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            Window.Title = title;
            Content.RootDirectory = "Content";
            scene = new Scene(Content);

            Application.Content = Content;
            Application.GraphicsDevice = GraphicsDevice;
            Application.Game = this;

            Screen.Setup(width, height, false, true);

            graphics.PreparingDeviceSettings += OnResize;
        }

        private void OnResize(object sender, PreparingDeviceSettingsEventArgs e)
        {
            int width = e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth;
            int height = e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight;

            Screen.Setup(width, height, null, null);
        }

        protected override void Initialize()
        {
            if (Application.GraphicsDevice == null)
                Application.GraphicsDevice = GraphicsDevice;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderer = new Renderer(GraphicsDevice);
            renderer.LoadContent(Content);

            Input.Keys = new KeyboardComponent(this);
            Input.Mouse = new MouseComponent(this);
            Input.Gamepad = new GamepadComponent(this);

            Components.Add(new Time(this));
            Components.Add(Input.Keys);
            Components.Add(Input.Mouse);
            Components.Add(Input.Gamepad);
           
            base.Initialize();
        }

        protected override void LoadContent()
        {
            scene.LoadContent(Content);
        }

        protected override void BeginRun()
        {
            base.BeginRun();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            scene.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            renderer.render(scene, scene.MainCamera);
            base.Draw(gameTime);
        }

        protected override void EndDraw()
        {
            base.EndDraw();

            if (Screen.LockCursor)
                Mouse.SetPosition(Screen.WidthPerTwo, Screen.HeightPerTwo);
        }
    }
}
