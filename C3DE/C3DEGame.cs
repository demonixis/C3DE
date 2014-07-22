using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE
{
    public class C3DEGame : Engine
    {
        private SceneObject spaceShip;

        public C3DEGame()
            : base()
        {
            Window.Title = "C3DE - Shadow Mapping";
        }

        protected override void LoadContent()
        {
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
            base.Update(gameTime);

            spaceShip.Transform.Rotate(0, 0.05f, 0);

            foreach (Transform tr in spaceShip.Transform.Transforms)
                tr.Rotate(0, 0, -0.05f);

            camPosition = Vector3.Zero;
            camRotation = Vector3.Zero;

            if (Input.Keys.Escape || Input.Gamepad.Pressed(Buttons.Back))
                Exit();

            // Move the light (oh it's so great \:D/)
            if (Input.Keys.Pressed(Keys.NumPad8) || Input.Gamepad.Pressed(Buttons.DPadUp))
                renderer.Light.Transform.Translate(0, 0, 0.1f);

            else if (Input.Keys.Pressed(Keys.NumPad5) || Input.Gamepad.Pressed(Buttons.DPadDown))
                renderer.Light.Transform.Translate(0, 0, -0.1f);

            if (Input.Keys.Pressed(Keys.NumPad4) || Input.Gamepad.Pressed(Buttons.DPadLeft))
                renderer.Light.Transform.Translate(0.1f, 0, 0);

            else if (Input.Keys.Pressed(Keys.NumPad6) || Input.Gamepad.Pressed(Buttons.DPadRight))
                renderer.Light.Transform.Translate(-0.1f, 0, 0);

            // Camera
            if (Input.Keys.Up || Input.Keys.Pressed(Keys.W))
                camPosition.Z += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (Input.Keys.Pressed(Keys.Down) || Input.Keys.Pressed(Keys.S))
                camPosition.Z -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            if (Input.Keys.Pressed(Keys.A))
                camPosition.X += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (Input.Keys.Pressed(Keys.D))
                camPosition.X -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            if (Input.Keys.Pressed(Keys.Q))
                camPosition.Y += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (Input.Keys.Pressed(Keys.E))
                camPosition.Y -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            if (Input.Keys.Pressed(Keys.PageUp))
                camRotation.X -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (Input.Keys.Pressed(Keys.PageDown))
                camRotation.X += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            if (Input.Keys.Pressed(Keys.Left))
                camRotation.Y += 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            else if (Input.Keys.Pressed(Keys.Right))
                camRotation.Y -= 0.01f * gameTime.ElapsedGameTime.Milliseconds;

            // Hello gamepad
            camPosition.X -= Input.Gamepad.ThumbSticks().X * 0.005f * gameTime.ElapsedGameTime.Milliseconds;
            camPosition.Z += Input.Gamepad.ThumbSticks().Y * 0.005f * gameTime.ElapsedGameTime.Milliseconds;
            camRotation.X -= Input.Gamepad.ThumbSticks(true).Y * 0.0015f * gameTime.ElapsedGameTime.Milliseconds;
            camRotation.Y -= Input.Gamepad.ThumbSticks(true).X * 0.0015f * gameTime.ElapsedGameTime.Milliseconds;

            // Dead zone for fucked sticks.
            if (Input.Gamepad.IsConnected())
            {
                camRotation.X = Math.Abs(camRotation.X) < 0.4f ? 0.0f : camRotation.X;
                camRotation.Y = Math.Abs(camRotation.Y) < 0.4f ? 0.0f : camRotation.Y;
            }

            // Apply translation and rotation.
            mainCamera.Translate(ref camPosition);
            mainCamera.Rotate(ref camRotation);

            if (Input.Keys.Pressed(Keys.J))
                spaceShip.Transform.Translate(0.1f, 0, 0);

            else if (Input.Keys.Pressed(Keys.L))
                spaceShip.Transform.Translate(-0.1f, 0, 0);

            if (Input.Keys.Pressed(Keys.I))
                spaceShip.Transform.Translate(0, 0, 0.1f);

            else if (Input.Keys.Pressed(Keys.K))
                spaceShip.Transform.Translate(0, 0, -0.1f);
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
