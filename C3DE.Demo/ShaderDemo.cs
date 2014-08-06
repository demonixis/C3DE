using C3DE.Components;
using C3DE.Components;
using C3DE.Components.Controllers;
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

            scene.AmbientColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

            // Camera
            var camera = new CameraPrefab("camera", scene);
            camera.AddComponent<OrbitController>();

            // Light
            var lightPrefab = new LightPrefab("light", LightType.Directional, scene);
            lightPrefab.Transform.Position = new Vector3(0, 15, 15);
            lightPrefab.Light.Range = 50;
            lightPrefab.Light.Intensity = 2.0f;
            lightPrefab.Light.DiffuseColor = Color.Violet;
            lightPrefab.Light.Direction = new Vector3(0, 1, -1);
            lightPrefab.Light.Angle = 120;
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(GraphicsDevice, 4096);
            lightPrefab.EnableShadows = true;
            lightTransform = lightPrefab.Transform;

            var lightSphere = lightPrefab.AddComponent<MeshRenderer>();
            lightSphere.Geometry = new SphereGeometry(1f, 8);
            lightSphere.Geometry.Generate(GraphicsDevice);
            lightSphere.Material = new SimpleMaterial(scene);
            lightSphere.Material.MainTexture = GraphicsHelper.CreateTexture(Color.Yellow, 1, 1);

            // Terrain
            var terrain = new TerrainPrefab("terrain", scene);
            terrain.TextureRepeat = new Vector2(8);
            terrain.Randomize();
            terrain.Renderer.Material = new SuperMaterial(scene);
            terrain.Renderer.Material.MainTexture = Content.Load<Texture2D>("Textures/terrainTexture");
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);

            // Cube
            var cubeSuperMaterial = new SuperMaterial(scene);
            cubeSuperMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.Red, Color.White); //Content.Load<Texture2D>("Textures/tech_box2");
            cubeSuperMaterial.DiffuseColor = new Color(0.8f, 0.8f, 1.0f, 1.0f);
            cubeSuperMaterial.SpecularColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            cubeSuperMaterial.Shininess = 500;
            cubeSuperMaterial.EmissiveColor = new Color(0f, 0.5f, 0.0f, 0.1f);

            var cubeScene = new SceneObject();
            cubeScene.Transform.Translate(0, 5.5f, 3);
            cubeScene.Transform.LocalScale = new Vector3(2.0f);
            cubeScene.Transform.Rotate((float)Math.PI / 4, 0, (float)Math.PI / 4);
            var autoRot = cubeScene.AddComponent<AutoRotation>();
            autoRot.Rotation = new Vector3(0, 0.01f, 0);
            scene.Add(cubeScene);

            var cube = cubeScene.AddComponent<MeshRenderer>();
            cube.RecieveShadow = false;
            cube.Geometry = new CubeGeometry();
            cube.Geometry.Generate(GraphicsDevice);
            cube.Material = cubeSuperMaterial;

            // Second cube
            var simpleMaterial = new SimpleMaterial(scene);
            simpleMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(new Color(1, 0, 0, 0.3f), new Color(1, 1, 1, 0.3f));
            simpleMaterial.Alpha = 0.3f;

            var cube2Scene = new SceneObject();
            cube2Scene.Transform.Translate(-10, 5.5f, -8);
            cube2Scene.Transform.LocalScale = new Vector3(3.0f);
            cube2Scene.Transform.Rotate(-MathHelper.PiOver4, 0, -MathHelper.PiOver4);
            var autoRot2 = cube2Scene.AddComponent<AutoRotation>();
            autoRot2.Rotation = new Vector3(0.02f, 0.01f, 0.03f);
            scene.Add(cube2Scene);

            var cube2 = cube2Scene.AddComponent<MeshRenderer>();
            cube2.RecieveShadow = false;
            cube2.Geometry = new CubeGeometry();
            cube2.Geometry.Generate(GraphicsDevice);
            cube2.Material = simpleMaterial;

            // Skybox
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
