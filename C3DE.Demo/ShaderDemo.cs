using C3DE.Components;
using C3DE.Components.Cameras;
using C3DE.Components.Lights;
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
    public class ShaderDemo : Engine
    {
        Transform lightTransform;

        public ShaderDemo()
            : base()
        {
            Window.Title = "C3DE - Shader demo";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();

            var materials = Demo.CreateMaterials(Content, scene);

            var camera = new CameraPrefab("camera", scene);
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            camera.AddComponent<OrbitController>();

            // And a light
            var light = new LightPrefab("light", LightType.Point, scene);
            light.EnableShadows = true;
            lightTransform = light.Transform;

            var terrain = new TerrainPrefab("terrain", scene);
            terrain.TextureRepeat = new Vector2(8);
            terrain.Randomize();
            terrain.Renderer.Material = new StandardMaterial(scene);
            terrain.Renderer.Material.MainTexture = Content.Load<Texture2D>("Textures/terrainTexture");
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);

            var material = new StandardMaterial(scene);
            material.MainTexture = Content.Load<Texture2D>("Textures/tech_box2");

            var cubeScene = new SceneObject();
            cubeScene.Transform.Translate(0, 2.5f, 3);
            cubeScene.Transform.LocalScale = new Vector3(2.0f);
            cubeScene.Transform.Rotate((float)Math.PI / 4, 0, (float)Math.PI / 4);
            var autoRot = cubeScene.AddComponent<AutoRotation>();
            autoRot.Rotation = new Vector3(0, 0.01f, 0);
            scene.Add(cubeScene);

            var cube = cubeScene.AddComponent<MeshRenderer>();
            cube.Geometry = new CubeGeometry();
            cube.Geometry.Generate(GraphicsDevice);
            cube.Material = material;

            renderer.Skybox.Generate(GraphicsDevice, Content, new string[] {
                "Textures/Skybox/px",   
                "Textures/Skybox/nx",
                "Textures/Skybox/py",
                "Textures/Skybox/ny",
                "Textures/Skybox/pz",
                "Textures/Skybox/nz"
            });

            Screen.ShowCursor = true;
        }

        // Just for tests, it's ugly, I know that ;)
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.Keys.Escape || Input.Gamepad.Pressed(Buttons.Back))
                Exit();

            // Move the light (oh it's so great \:D/)
            if (Input.Keys.Pressed(Keys.NumPad8) || Input.Gamepad.Pressed(Buttons.DPadUp))
                lightTransform.Translate(0, 0.1f, 0.0f);

            else if (Input.Keys.Pressed(Keys.NumPad5) || Input.Gamepad.Pressed(Buttons.DPadDown))
                lightTransform.Translate(0, -0.1f, 0.0f);

            // Move the light (oh it's so great \:D/)
            if (Input.Keys.Pressed(Keys.NumPad7) || Input.Gamepad.Pressed(Buttons.DPadUp))
                lightTransform.Translate(0, 0.0f, 0.1f);

            else if (Input.Keys.Pressed(Keys.NumPad9) || Input.Gamepad.Pressed(Buttons.DPadDown))
                lightTransform.Translate(0, 0.0f, -0.1f);

            if (Input.Keys.Pressed(Keys.NumPad4) || Input.Gamepad.Pressed(Buttons.DPadLeft))
                lightTransform.Translate(0.1f, 0, 0);

            else if (Input.Keys.Pressed(Keys.NumPad6) || Input.Gamepad.Pressed(Buttons.DPadRight))
                lightTransform.Translate(-0.1f, 0, 0);

            if (Input.Keys.Enter)
                Console.WriteLine(lightTransform.Position);
        }
    }
}
