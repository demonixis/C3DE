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
        TerrainPrefab terrain;
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
            controller.AngularVelocity = new Vector3(0.01f, 0.9f, 0);
            controller.Velocity = new Vector3(0.9f, 0.4f, 0.9f);
            controller.MoveSpeed = 0.001f;
            controller.RotationSpeed = 0.0005f;

            var sceneLight = new SceneObject();
            scene.Add(sceneLight);

            lightTransform = sceneLight.Transform;

            var light = sceneLight.AddComponent<Light>();
            light.ShadowGenerator.Enabled = true;
            light.ShadowGenerator.SetShadowMapSize(GraphicsDevice, 1024);

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
                mr.RecieveShadow = false;
                mr.Material = materials["box2"];
            }

            terrain = new TerrainPrefab("terrain");
            terrain.TextureRepeat = new Vector2(16);
            terrain.Randomize(GraphicsDevice);
            //terrain.LoadHeightmap(GraphicsDevice, Content.Load<Texture2D>("Textures/heightmap2"));
            scene.Add(terrain);

            terrain.Renderer.Material = materials["terrain2"];
            terrain.Transform.Translate(-terrain.Renderer.BoundingSphere.Radius / 2, 0, -terrain.Renderer.BoundingSphere.Radius / 2);

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
