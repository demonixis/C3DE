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
    public class GridDemo : Engine
    {
        LightPrefab light;

        public GridDemo()
            : base()
        {
            Window.Title = "C3DE - Grid demo";
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Camera
            var camera = new CameraPrefab("camera", scene);
            camera.AddComponent<OrbitController>();

            // Light
            light = new LightPrefab("light", LightType.Point, scene);
            light.Transform.Position = new Vector3(0, 15, 15);
            light.Light.Range = 25;
            light.Light.Intensity = 2.0f;
            light.Light.FallOf = 5f;
            light.Light.DiffuseColor = Color.Violet;
            light.Light.Direction = new Vector3(-1, 1, -1);
            light.Light.Angle = 0.1f;
            light.Light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
            light.Light.ShadowGenerator.SetShadowMapSize(GraphicsDevice, 1024);
            light.EnableShadows = true;

            var lightSphere = light.AddComponent<MeshRenderer>();
            lightSphere.Geometry = new SphereGeometry(2f, 4);
            lightSphere.Geometry.Generate(GraphicsDevice);
            lightSphere.CastShadow = false;
            lightSphere.RecieveShadow = false;
            lightSphere.Material = new SimpleMaterial(scene);
            lightSphere.Material.MainTexture = GraphicsHelper.CreateTexture(Color.Yellow, 1, 1);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = GraphicsHelper.CreateBorderTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128, 1);
            terrainMaterial.Shininess = 10;

            var terrain = new TerrainPrefab("terrain", scene);
            terrain.TextureRepeat = new Vector2(16);
            terrain.Flat();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);

            // Cube
            var cubeSuperMaterial = new StandardMaterial(scene);
            cubeSuperMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.FloralWhite, Color.DodgerBlue); //Content.Load<Texture2D>("Textures/tech_box2");
            cubeSuperMaterial.DiffuseColor = Color.WhiteSmoke;
            cubeSuperMaterial.SpecularColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            cubeSuperMaterial.Shininess = 10;
            cubeSuperMaterial.EmissiveColor = new Color(0f, 0.0f, 0.1f, 1.0f);

            var cubeScene = new SceneObject();
            cubeScene.Transform.Translate(0, 5.5f, 0);
            cubeScene.Transform.LocalScale = new Vector3(1.0f);
            cubeScene.Transform.Rotate((float)Math.PI / 4, 0, (float)Math.PI / 4);
            var autoRot = cubeScene.AddComponent<AutoRotation>();
            autoRot.Rotation = new Vector3(0, 0.01f, 0);
            scene.Add(cubeScene);

            var cube = cubeScene.AddComponent<MeshRenderer>();
            cube.RecieveShadow = false;
            cube.Geometry = new CubeGeometry();
            cube.Geometry.Generate(GraphicsDevice);
            cube.Material = cubeSuperMaterial;

            var c2 = new SceneObject();
            cubeScene.Add(c2);

            //c2.AddComponent<AutoRotation>().Rotation = new Vector3(0.0f, 0.0f, -0.03f);
            c2.Transform.Translate(0, 3, 0);
            c2.Transform.LocalScale = new Vector3(5);
            var m2 = c2.AddComponent<MeshRenderer>();
            m2.Geometry = new CubeGeometry();
            m2.Geometry.Generate(GraphicsDevice);
            m2.Material = cubeSuperMaterial;

            // Skybox
            renderer.Skybox.Generate(GraphicsDevice, Content, new string[] {
                "Textures/Skybox/starfield/px",   
                "Textures/Skybox/starfield/nx",
                "Textures/Skybox/starfield/py",
                "Textures/Skybox/starfield/ny",
                "Textures/Skybox/starfield/pz",
                "Textures/Skybox/starfield/nz"
            });

            Screen.ShowCursor = true;
        }

        Vector3 translation;

        // Just for tests, it's ugly, I know that ;)
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.Keys.Escape || Input.Gamepad.Pressed(Buttons.Back))
                Exit();

            translation = Vector3.Zero;

            if (Input.Mouse.Down(Inputs.MouseButton.Middle))
                translation.Y += Input.Mouse.Delta.Y * 0.1f;
            else
                translation.Z += Input.Mouse.Delta.Y * 0.1f;

            translation.X += Input.Mouse.Delta.X * 0.1f;

            if (Input.Keys.JustPressed(Keys.F1))
                light.Light.Type = LightType.Ambient;
            else if (Input.Keys.JustPressed(Keys.F2))
                light.Light.Type = LightType.Directional;
            else if (Input.Keys.JustPressed(Keys.F3))
                light.Light.Type = LightType.Point;
            else if (Input.Keys.JustPressed(Keys.F4))
                light.Light.Type = LightType.Spot;

            if (Input.Keys.Pressed(Keys.Add))
                light.Light.Range += 0.1f;
            else if (Input.Keys.Pressed(Keys.Subtract))
                light.Light.Range -= 0.1f;

            if (Input.Keys.Pressed(Keys.Divide))
                light.Light.Intensity += 0.1f;
            else if (Input.Keys.Pressed(Keys.Multiply))
                light.Light.Intensity -= 0.1f;

            if (Input.Keys.Pressed(Keys.P))
                light.Light.FallOf += 0.1f;
            else if (Input.Keys.Pressed(Keys.M))
                light.Light.FallOf -= 0.1f;

            light.Transform.Translate(ref translation);
        }
    }
}
