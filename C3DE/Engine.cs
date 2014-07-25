using C3DE.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE
{
    public class App
    {
        public static ContentManager Content { get; internal set; }
        public static GraphicsDevice GraphicsDevice { get; internal set; }
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
        protected CameraPrefab mainCamera;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 600;
            Window.Title = "C3DE";
            Content.RootDirectory = "Content";
            scene = new Scene();
        }

        protected override void Initialize()
        {
            base.Initialize();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            mainCamera = new CameraPrefab(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            renderer = new Renderer(GraphicsDevice);
            renderer.LoadContent(Content);

            App.Content = Content;
            App.GraphicsDevice = GraphicsDevice;

            Input.Keys = new KeyboardComponent(this);
            Input.Mouse = new MouseComponent(this);
            Input.Gamepad = new GamepadComponent(this);

            Components.Add(Input.Keys);
            Components.Add(Input.Mouse);
            Components.Add(Input.Gamepad);

            scene.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            mainCamera.Update();
            scene.Update();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            renderer.render(scene, mainCamera);
            base.Draw(gameTime);
        }
    }
}
