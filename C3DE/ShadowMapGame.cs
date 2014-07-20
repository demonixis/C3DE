using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C3DE
{
    public class ShadowMapGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Renderer renderer;
        Scene scene;
        Camera camera;
        SceneObject spaceShip;

        public ShadowMapGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 600;
            Window.Title = "C3DE - Shadow Mapping";
            Content.RootDirectory = "Content";
            scene = new Scene();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            camera = new Camera(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            renderer = new Renderer(GraphicsDevice);
            renderer.LoadContent(Content);

            ModelRenderer modelRenderer = null;

            //ship
            spaceShip = new SceneObject();
            spaceShip.Transform.Scale = new Vector3(0.25f);
            scene.Add(spaceShip);

            modelRenderer = spaceShip.AddComponent<ModelRenderer>();
            modelRenderer.Model = Content.Load<Model>("Models/spaceship");
            modelRenderer.Texture = Content.Load<Texture2D>("Models/texv1");

            // Ship 2
            SceneObject sceneObject = new SceneObject();
            sceneObject.Transform.Translate(6, 0, 0);
            sceneObject.Transform.Scale = new Vector3(16);
            sceneObject.Transform.Rotate(-MathHelper.PiOver2, 0, 0);
            spaceShip.Add(sceneObject);

            modelRenderer = sceneObject.AddComponent<ModelRenderer>();
            modelRenderer.Model = Content.Load<Model>("Models/ship");
            modelRenderer.Texture = Content.Load<Texture2D>("Textures/solarPanel");


            // Ball
            sceneObject = new SceneObject();
            sceneObject.Transform.Translate(-6, 0, 0);
            spaceShip.Add(sceneObject);

            modelRenderer = sceneObject.AddComponent<ModelRenderer>();
            modelRenderer.Model = Content.Load<Model>("Models/ball");
            modelRenderer.Texture = Content.Load<Texture2D>("Textures/huleShip");


            // Floor
            sceneObject = new SceneObject();
            sceneObject.Transform.Rotate(MathHelper.PiOver2, 0, 0);
            sceneObject.Transform.Translate(0, -1, 0);
            sceneObject.Transform.Scale = new Vector3(4);
            scene.Add(sceneObject);

            modelRenderer = sceneObject.AddComponent<ModelRenderer>();
            modelRenderer.Model = Content.Load<Model>("Models/Floor");
            modelRenderer.Texture = Content.Load<Texture2D>("Textures/huleShip");
            modelRenderer.CastShadow = false;
        }

        // Just for tests, it's ugly, I know that ;)
        protected override void Update(GameTime gameTime)
        {
            camera.Update();
            scene.Update();

            spaceShip.Transform.Rotate(0, 0.05f, 0);

            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            // Camera
            if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))
                camera.Translate(0, 0, 0.1f);

            else if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))
                camera.Translate(0, 0, -0.1f);

            if (state.IsKeyDown(Keys.D))
                camera.Translate(-0.1f, 0.0f, 0);

            else if (state.IsKeyDown(Keys.A))
                camera.Translate(0.1f, 0.0f, 0);

            if (state.IsKeyDown(Keys.Left))
                camera.Rotate(0, 0.1f, 0);

            else if (state.IsKeyDown(Keys.Right))
                camera.Rotate(0, -0.1f, 0);

            if (state.IsKeyDown(Keys.Q))
                camera.Translate(0, -0.1f, 0);

            else if (state.IsKeyDown(Keys.E))
                camera.Translate(0, 0.1f, 0);

            if (state.IsKeyDown(Keys.PageUp))
                camera.Rotate(0.05f, 0.0f, 0);

            else if (state.IsKeyDown(Keys.PageDown))
                camera.Rotate(-0.05f, 0.0f, 0);

            // Ship
            if (state.IsKeyDown(Keys.NumPad4))
                spaceShip.Transform.Translate(0, 0.1f, 0);

            else if (state.IsKeyDown(Keys.NumPad6))
                spaceShip.Transform.Translate(0, -0.1f, 0);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            renderer.render(scene, camera);
            base.Draw(gameTime);
        }
    }

    // Entry point.
    static class Program
    {
        static void Main(string[] args)
        {
            using (ShadowMapGame game = new ShadowMapGame())
                game.Run();
        }
    }
}
