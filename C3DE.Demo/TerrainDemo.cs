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
    public class TerrainDemo : Engine
    {
        Transform lightTransform;

        public TerrainDemo()
            : base()
        {
            Window.Title = "C3DE - Terrain";
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

            var controller = camera.AddComponent<FirstPersonController>();

            var sceneLight = new SceneObject();
            scene.Add(sceneLight);

            var light = sceneLight.AddComponent<Light>();
            light.ShadowGenerator.Enabled = true;
            light.ShadowGenerator.SetShadowMapSize(GraphicsDevice, 1024);

            lightTransform = sceneLight.Transform;

            var terrain = new TerrainPrefab("terrain");
            terrain.TextureRepeat = new Vector2(16);
            //terrain.Randomize(GraphicsDevice);
            terrain.Transform.Translate(0, -10, 0);
            terrain.LoadHeightmap(GraphicsDevice, Content.Load<Texture2D>("Textures/heightmap"));
            //terrain.Renderer.RecieveShadow = false;
            scene.Add(terrain);

            terrain.Renderer.Material = materials["terrain2"];
            terrain.Transform.Translate(-terrain.Renderer.BoundingSphere.Radius / 2, 0, -terrain.Renderer.BoundingSphere.Radius / 2);

            var waterPlane = new SceneObject();
            waterPlane.Transform.Translate(0, 0, 0);
            scene.Add(waterPlane);

            var waterMaterial = new WaterMaterial(scene);
            waterMaterial.MainTexture = Content.Load<Texture2D>("Textures/water");
            waterMaterial.BumpTexture = Content.Load<Texture2D>("Textures/wavesbump");

            var waterRenderer = waterPlane.AddComponent<MeshRenderer>();
            //waterRenderer.CastShadow = false;
            waterRenderer.RecieveShadow = false;
            waterRenderer.Geometry = new PlaneGeometry();
            //waterRenderer.Geometry.TextureRepeat = new Vector2(32);
            waterRenderer.Geometry.Size = new Vector3(terrain.Renderer.BoundingSphere.Radius * 0.5f);
            waterRenderer.Geometry.Generate(GraphicsDevice);
            waterRenderer.Material = waterMaterial;

            renderer.Skybox.LoadContent(Content);
            renderer.Skybox.Generate(GraphicsDevice, new Texture2D[] {
                Content.Load<Texture2D>("Textures/Skybox/px"),   
                Content.Load<Texture2D>("Textures/Skybox/nx"),
                Content.Load<Texture2D>("Textures/Skybox/py"),
                Content.Load<Texture2D>("Textures/Skybox/ny"),
                Content.Load<Texture2D>("Textures/Skybox/pz"),
                Content.Load<Texture2D>("Textures/Skybox/nz")
            });

            Input.Gamepad.Sensitivity = new Vector2(1, 0.75f);
      
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
                lightTransform.Translate(0, 0, 0.1f);

            else if (Input.Keys.Pressed(Keys.NumPad5) || Input.Gamepad.Pressed(Buttons.DPadDown))
                lightTransform.Translate(0, 0, -0.1f);

            if (Input.Keys.Pressed(Keys.NumPad4) || Input.Gamepad.Pressed(Buttons.DPadLeft))
                lightTransform.Translate(0.1f, 0, 0);

            else if (Input.Keys.Pressed(Keys.NumPad6) || Input.Gamepad.Pressed(Buttons.DPadRight))
                lightTransform.Translate(-0.1f, 0, 0);
        }
    }
}
