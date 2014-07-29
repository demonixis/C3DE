using C3DE.Components;
using C3DE.Components.Cameras;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
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
            material.MainTexture = Content.Load<Texture2D>("Textures/marsTexture");
            materials.Add("mars", material);

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

            var camera = new CameraPrefab("camera");
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            scene.Add(camera);

            var controller = camera.AddComponent<FirstPersonController>();
            controller.AngularVelocity = new Vector3(0.9f);
            controller.Velocity = new Vector3(0.9f);
            controller.MoveSpeed = 0.001f;
            controller.RotationSpeed = 0.0005f;

            SceneObject so = null;
            MeshRenderer mr = null;
            AutoRotation ar = null;

            for (int i = 0; i < 10; i++)
            {
                so = new SceneObject();
                so.Transform.Translate(RandomHelper.GetVector3(-50, 2, -50, 50, 3, 50));
                so.Transform.Rotate(RandomHelper.GetVector3(0, 0, 0, 0, (float)Math.PI, 0));
                scene.Add(so);

                ar = so.AddComponent<AutoRotation>();
                ar.Rotation = new Vector3(0, 0.05f, 0);

                mr = so.AddComponent<MeshRenderer>();
                mr.Geometry = new CubeGeometry();
                mr.Geometry.Generate(GraphicsDevice);
                mr.ComputeBoundingSphere();
                mr.Material = RandomHelper.Range(1, 5) % 2 == 0 ? materials["box"] : materials["box2"];
            }

            terrain = new TerrainPrefab("terrain");
            terrain.TextureRepeat = new Vector2(16);
            //terrain.Flat(GraphicsDevice);
            terrain.Randomize(GraphicsDevice);
            //terrain.LoadHeightmap(GraphicsDevice, Content.Load<Texture2D>("Textures/heightmap"));
            scene.Add(terrain);

            terrain.Renderer.Material = materials["mars"];
            terrain.Transform.Translate(-terrain.Renderer.BoundingSphere.Radius / 2, 0, -terrain.Renderer.BoundingSphere.Radius / 2);
            //terrain.ApplyCollision(ref mainCamera.Transform.Position);

            this.IsMouseVisible = true;
        }

        // Just for tests, it's ugly, I know that ;)
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
