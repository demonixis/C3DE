using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE
{
    public class C3DEGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Renderer renderer;
        Scene scene;
        Camera camera;
        SceneObject spaceShip;

        public C3DEGame()
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
            spaceShip.Transform.Translate(0, 1, 0);
            spaceShip.Transform.Scale = new Vector3(0.25f);
            scene.Add(spaceShip);

            modelRenderer = spaceShip.AddComponent<ModelRenderer>();
            modelRenderer.Model = Content.Load<Model>("Models/spaceship");
            modelRenderer.MainTexture = Content.Load<Texture2D>("Models/texv1");

            var boxCollider = spaceShip.AddComponent<BoxCollider>();
            boxCollider.Update();

            // Ship 2
            SceneObject sceneObject = new SceneObject();
            sceneObject.Transform.Translate(12, -4f, 0);
            sceneObject.Transform.Scale = new Vector3(4);
            sceneObject.Transform.Rotate(-MathHelper.PiOver2, 0, 0);
            spaceShip.Add(sceneObject);

            modelRenderer = sceneObject.AddComponent<ModelRenderer>();
            modelRenderer.Model = Content.Load<Model>("Models/ship");
            modelRenderer.MainTexture = Content.Load<Texture2D>("Textures/huleShip");


            // Ship 3
            sceneObject = new SceneObject();
            sceneObject.Transform.Translate(-12, -4f, 0);
            sceneObject.Transform.Scale = new Vector3(4);
            sceneObject.Transform.Rotate(-MathHelper.PiOver2, 0, 0);
            spaceShip.Add(sceneObject);

            modelRenderer = sceneObject.AddComponent<ModelRenderer>();
            modelRenderer.Model = Content.Load<Model>("Models/ship");
            modelRenderer.MainTexture = Content.Load<Texture2D>("Textures/huleShip");


            // Floor
            sceneObject = new SceneObject();
            sceneObject.Transform.Rotate(0, MathHelper.PiOver2, 0);
            sceneObject.Transform.Translate(-2, -1.5f, -15);
            scene.Add(sceneObject);

            modelRenderer = sceneObject.AddComponent<ModelRenderer>();
            modelRenderer.Model = Content.Load<Model>("Models/Level/DockStation");
            modelRenderer.MainTexture = Content.Load<Texture2D>("Models/Level/hullTexture_2_0");
            modelRenderer.CastShadow = false;
        }

        Vector3 camPosition = Vector3.Zero;
        Vector3 camRotation = Vector3.Zero;

        // Just for tests, it's ugly, I know that ;)
        protected override void Update(GameTime gameTime)
        {
            camera.Update();
            scene.Update();

            spaceShip.Transform.Rotate(0, 0.05f, 0);

            foreach (Transform tr in spaceShip.Transform.Transforms)
            {
                tr.Rotate(0, 0, -0.05f);
            }

            camPosition = Vector3.Zero;
            camRotation = Vector3.Zero;

            var state = Keyboard.GetState();
            var pad = GamePad.GetState(PlayerIndex.One);

            if (state.IsKeyDown(Keys.Escape) || pad.IsButtonDown(Buttons.Back))
                Exit();

            // Move the light (oh it's so great \:D/)
            if (pad.IsButtonDown(Buttons.DPadLeft) || state.IsKeyDown(Keys.NumPad4))
                renderer.Light.Transform.Translate(0.1f, 0, 0);

            else if (pad.IsButtonDown(Buttons.DPadRight) || state.IsKeyDown(Keys.NumPad6))
                renderer.Light.Transform.Translate(-0.1f, 0, 0);

            if (pad.IsButtonDown(Buttons.DPadUp) || state.IsKeyDown(Keys.NumPad8))
                renderer.Light.Transform.Translate(0, 0, 0.1f);

            else if (pad.IsButtonDown(Buttons.DPadDown) || state.IsKeyDown(Keys.NumPad5))
                renderer.Light.Transform.Translate(0, 0, -0.1f);

            // Camera
            if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))
                camPosition.Z += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))
                camPosition.Z -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            if (state.IsKeyDown(Keys.A))
                camPosition.X += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (state.IsKeyDown(Keys.D))
                camPosition.X -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            if (state.IsKeyDown(Keys.Q))
                camPosition.Y += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (state.IsKeyDown(Keys.E))
                camPosition.Y -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            if (state.IsKeyDown(Keys.PageUp))
                camRotation.X -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (state.IsKeyDown(Keys.PageDown))
                camRotation.X += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            if (state.IsKeyDown(Keys.Left))
                camRotation.Y += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (state.IsKeyDown(Keys.Right))
                camRotation.Y -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            // Hello gamepad
            camPosition.X -= pad.ThumbSticks.Left.X * 0.005f * gameTime.ElapsedGameTime.Milliseconds;
            camPosition.Z += pad.ThumbSticks.Left.Y * 0.005f * gameTime.ElapsedGameTime.Milliseconds;
            camRotation.X -= pad.ThumbSticks.Right.Y * 0.0015f * gameTime.ElapsedGameTime.Milliseconds;
            camRotation.Y -= pad.ThumbSticks.Right.X * 0.0015f * gameTime.ElapsedGameTime.Milliseconds;

            // Dead zone for fucked sticks.
            if (pad.IsConnected)
            {
                camRotation.X = Math.Abs(pad.ThumbSticks.Right.Y) < 0.4f ? 0.0f : camRotation.X;
                camRotation.Y = Math.Abs(pad.ThumbSticks.Right.X) < 0.4f ? 0.0f : camRotation.Y;
            }

            // Apply translation and rotation.
            camera.Translate(ref camPosition);
            camera.Rotate(ref camRotation);

            if (state.IsKeyDown(Keys.J))
                spaceShip.Transform.Translate(0.1f, 0, 0);

            else if (state.IsKeyDown(Keys.L))
                spaceShip.Transform.Translate(-0.1f, 0, 0);

            if (state.IsKeyDown(Keys.I))
                spaceShip.Transform.Translate(0, 0, 0.1f);

            else if (state.IsKeyDown(Keys.K))
                spaceShip.Transform.Translate(0, 0, -0.1f);

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
            using (C3DEGame game = new C3DEGame())
                game.Run();
        }
    }
}
