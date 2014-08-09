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
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Skybox
            renderer.Skybox.Generate(GraphicsDevice, Content, new string[] {
                "Textures/Skybox/bluesky/px",   
                "Textures/Skybox/bluesky/nx",
                "Textures/Skybox/bluesky/py",
                "Textures/Skybox/bluesky/ny",
                "Textures/Skybox/bluesky/pz",
                "Textures/Skybox/bluesky/nz"
            });

            // Camera
            var camera = new CameraPrefab("camera", scene);
            camera.AddComponent<OrbitController>();

            // Light
            var lightPrefab = new LightPrefab("light", LightType.Directional, scene);
            lightPrefab.Transform.Position = new Vector3(0, 15, 15);
            lightPrefab.Light.Range = 25;
            lightPrefab.Light.Intensity = 2.0f;
            lightPrefab.Light.FallOf = 5f;
            lightPrefab.Light.DiffuseColor = Color.Violet;
            lightPrefab.Light.Direction = new Vector3(0, 1, -1);
            lightPrefab.Light.Angle = MathHelper.PiOver4;
            lightPrefab.Light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            lightTransform = lightPrefab.Transform;

            var lightSphere = lightPrefab.AddComponent<MeshRenderer>();
            lightSphere.Geometry = new SphereGeometry(1f, 8);
            lightSphere.Geometry.Generate(GraphicsDevice);
            lightSphere.CastShadow = false;
            lightSphere.RecieveShadow = false;
            lightSphere.Material = new SimpleMaterial(scene);
            lightSphere.Material.MainTexture = GraphicsHelper.CreateTexture(Color.Yellow, 1, 1);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Content.Load<Texture2D>("Textures/terrainTexture");
            terrainMaterial.Shininess = 500;
            
            var terrain = new TerrainPrefab("terrain", scene);
            terrain.TextureRepeat = new Vector2(8);
            terrain.Randomize();
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
            cubeScene.Transform.Translate(0, 5.5f, 15);
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

            // Second cube
            var simpleMaterial = new SimpleMaterial(scene);
            simpleMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(new Color(1, 0, 0, 0.3f), new Color(1, 1, 1, 0.3f));
            simpleMaterial.Alpha = 0.3f;

            var cube2Scene = new SceneObject();
            cube2Scene.Transform.Translate(-20, 5.5f, -5);
            cube2Scene.Transform.LocalScale = new Vector3(3.0f);
            cube2Scene.Transform.Rotate(-MathHelper.PiOver4, 0, -MathHelper.PiOver4);
            var autoRot2 = cube2Scene.AddComponent<AutoRotation>();
            autoRot2.Rotation = new Vector3(0.02f, 0.01f, 0.03f);
            scene.Add(cube2Scene);

            var cube2 = cube2Scene.AddComponent<MeshRenderer>();
            cube2.RecieveShadow = false;
            cube2.CastShadow = true;
            cube2.Geometry = new CubeGeometry();
            cube2.Geometry.Generate(GraphicsDevice);
            cube2.Material = simpleMaterial;

            // Third cube
            var reflectiveMaterial = new ReflectiveMaterial(scene);
            reflectiveMaterial.MainTexture = cubeSuperMaterial.MainTexture;
            reflectiveMaterial.TextureCube = renderer.Skybox.Texture;
            reflectiveMaterial.DiffuseColor = Color.Red;

            var cube3Scene = new SceneObject();
            cube3Scene.Transform.Translate(0, 7.5f, -30);
            cube3Scene.Transform.LocalScale = new Vector3(4.0f);
            cube3Scene.Transform.Rotate(MathHelper.PiOver4, 0, -MathHelper.PiOver2);
            var autoRot3 = cube3Scene.AddComponent<AutoRotation>();
            autoRot3.Rotation = new Vector3(0.1f, 0.02f, 0.03f);
            scene.Add(cube3Scene);

            var cube3 = cube3Scene.AddComponent<MeshRenderer>();
            cube3.RecieveShadow = false;
            cube3.CastShadow = true;
            cube3.Geometry = new CubeGeometry();
            cube3.Geometry.Generate(GraphicsDevice);
            cube3.Material = reflectiveMaterial;

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

            lightTransform.Translate(ref translation);

            if (Input.Keys.Enter)
                Console.WriteLine(lightTransform.Position);
        }
    }
}
