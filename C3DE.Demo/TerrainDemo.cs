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

            // Add a camera with a FPS controller
            var camera = new CameraPrefab("camera", scene);
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            camera.AddComponent<FirstPersonController>();

            // And a light
            var light = new LightPrefab("light", LightType.Directional, scene);
            light.Light.Direction = new Vector3(1, 1, 0);
            light.EnableShadows = true;

            // Just for playing with light
            lightTransform = light.Transform;

            // Finally a terrain
            var terrainMat = new SuperMaterial(scene);
            terrainMat.MainTexture = Content.Load<Texture2D>("Textures/terrainTexture");
            terrainMat.Shininess = 50;

            var terrain = new TerrainPrefab("terrain", scene);
            terrain.TextureRepeat = new Vector2(2);
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMat;
            terrain.Transform.Translate(-terrain.Width >> 1, -10, -terrain.Depth >> 1);            

            // With water !
            var water = new WaterPrefab("water", scene);

            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));
            
            // Don't miss the Skybox ;)
            renderer.Skybox.Generate(GraphicsDevice, Content, new string[] {
                "Textures/Skybox/bluesky/px",   
                "Textures/Skybox/bluesky/nx",
                "Textures/Skybox/bluesky/py",
                "Textures/Skybox/bluesky/ny",
                "Textures/Skybox/bluesky/pz",
                "Textures/Skybox/bluesky/nz"
            });

            Input.Gamepad.Sensitivity = new Vector2(1, 0.75f);
            Screen.ShowCursor = true;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.Keys.Escape || Input.Gamepad.Pressed(Buttons.Back))
                Exit();

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
