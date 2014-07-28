using C3DE.Components;
using C3DE.Components.Cameras;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace C3DE.Demo
{
    public class C3DEGame : Engine
    {
        private Dictionary<string, Material> materials;
        Vector3 camPosition = Vector3.Zero;
        Vector3 camRotation = Vector3.Zero;
        TerrainPrefab terrain;

        public C3DEGame()
            : base()
        {
            Window.Title = "C3DE - Shadow Mapping";
        }

        private void CreateMaterials()
        {
            materials = new Dictionary<string, Material>(10);

            Material material = new Material(scene);
            material.MainTexture = Content.Load<Texture2D>("Textures/huleShip");
            materials.Add("huleShip", material);

            material = new Material(scene);
            material.MainTexture = Content.Load<Texture2D>("Models/texv1");
            materials.Add("spaceShip", material);

            material = new Material(scene);
            material.MainTexture = Content.Load<Texture2D>("Textures/tech_box");
            materials.Add("box", material);

            material = new Material(scene);
            material.MainTexture = Content.Load<Texture2D>("Textures/tech_box2");
            materials.Add("box2", material);

            material = new Material(scene);
            material.MainTexture = Content.Load<Texture2D>("Textures/heightmapTexture");
            materials.Add("terrain", material);

            var skyboxMaterial = new SkyboxMaterial(scene);
            skyboxMaterial.Textures = new Texture2D[6] {
                Content.Load<Texture2D>("Textures/Skybox/nx"),
                Content.Load<Texture2D>("Textures/Skybox/ny"),
                Content.Load<Texture2D>("Textures/Skybox/nz"),
                Content.Load<Texture2D>("Textures/Skybox/px"),
                Content.Load<Texture2D>("Textures/Skybox/py"),
                Content.Load<Texture2D>("Textures/Skybox/pz")
            };
        }

        protected override void Initialize()
        {
            base.Initialize();

            CreateMaterials();

            var soCamera = new SceneObject();
            scene.Add(soCamera);

            var camera = soCamera.AddComponent<Camera>();
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            
            // Cube
            var sceneObject = new SceneObject();
            sceneObject.Transform.Translate(0, 2, 0);
            scene.Add(sceneObject);

            var mesh = sceneObject.AddComponent<MeshRenderer>();
            mesh.Geometry = new CubeGeometry();
            mesh.Geometry.Generate(GraphicsDevice);
            mesh.ComputeBoundingSphere();
            mesh.Material = materials["box"];

            terrain = new TerrainPrefab("terrain");
            terrain.Flat(GraphicsDevice);
            //terrain.Randomize(GraphicsDevice);
            //terrain.LoadHeightmap(GraphicsDevice, Content.Load<Texture2D>("Textures/heightmap"));
            scene.Add(terrain);

            terrain.Renderer.Material = materials["terrain"];
            terrain.Transform.Translate(-terrain.Renderer.BoundingSphere.Radius / 2, 0, -terrain.Renderer.BoundingSphere.Radius / 2);
            //terrain.ApplyCollision(ref mainCamera.Transform.Position);

            this.IsMouseVisible = true;
        }

        // Just for tests, it's ugly, I know that ;)
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
                camRotation.X = Math.Abs(camRotation.X) < 0.1f ? 0.0f : camRotation.X;
                camRotation.Y = Math.Abs(camRotation.Y) < 0.1f ? 0.0f : camRotation.Y;
            }

            // Apply translation and rotation.
            //mainCamera.Translate(ref camPosition);
            //mainCamera.Rotate(ref camRotation);

            //terrain.ApplyCollision(ref mainCamera.position);
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
