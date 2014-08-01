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

            var camera = new CameraPrefab("camera");
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            scene.Add(camera);

            var controller = camera.AddComponent<OrbitController>();

            var sceneLight = new SceneObject();
            scene.Add(sceneLight);

            lightTransform = sceneLight.Transform;

            var light = sceneLight.AddComponent<Light>();
            light.ShadowGenerator.Enabled = true;
            light.ShadowGenerator.SetShadowMapSize(GraphicsDevice, 1024);

            var terrain = new TerrainPrefab("terrain");
            terrain.TextureRepeat = new Vector2(8);
            terrain.Randomize(GraphicsDevice);
            scene.Add(terrain);

            var terrainMaterial = new DiffuseSpecularMaterial(scene);
            terrainMaterial.MainTexture = Content.Load<Texture2D>("Textures/terrainTexture");

            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.CastShadow = true;
            terrain.Renderer.RecieveShadow = false;
            terrain.Transform.Translate(-terrain.Renderer.BoundingSphere.Radius / 2, 0, -terrain.Renderer.BoundingSphere.Radius / 2);

            var material = new DiffuseSpecularMaterial(scene);
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

            renderer.Skybox.LoadContent(Content);
            renderer.Skybox.Generate(GraphicsDevice, new Texture2D[] {
                Content.Load<Texture2D>("Textures/Skybox/px"),   
                Content.Load<Texture2D>("Textures/Skybox/nx"),
                Content.Load<Texture2D>("Textures/Skybox/py"),
                Content.Load<Texture2D>("Textures/Skybox/ny"),
                Content.Load<Texture2D>("Textures/Skybox/pz"),
                Content.Load<Texture2D>("Textures/Skybox/nz")
            });

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
                lightTransform.Translate(0, 0.1f, 0.0f);

            else if (Input.Keys.Pressed(Keys.NumPad5) || Input.Gamepad.Pressed(Buttons.DPadDown))
                lightTransform.Translate(0, -0.1f, 0.0f);

            if (Input.Keys.Pressed(Keys.NumPad4) || Input.Gamepad.Pressed(Buttons.DPadLeft))
                lightTransform.Translate(0.1f, 0, 0);

            else if (Input.Keys.Pressed(Keys.NumPad6) || Input.Gamepad.Pressed(Buttons.DPadRight))
                lightTransform.Translate(-0.1f, 0, 0);

            if (Input.Keys.Enter)
                Console.WriteLine(lightTransform.Position);
        }
    }
}
