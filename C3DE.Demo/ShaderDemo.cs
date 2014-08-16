using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo
{
    public class ShaderDemo : Engine
    {
        public ShaderDemo()
            : base()
        {
            Window.Title = "C3DE - Shader demo";
            graphics.PreferredBackBufferWidth = Demo.ScreenWidth;
            graphics.PreferredBackBufferHeight = Demo.ScreenHeight;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Skybox
            renderer.Skybox.Generate(GraphicsDevice, Content, Demo.BlueSkybox);

            // Camera
            var camera = new CameraPrefab("camera", scene);
            camera.AddComponent<ControllerSwitcher>();
            camera.AddComponent<DemoBehaviour>();
            camera.AddComponent<RayPickingTester>();

            // Light
            var lightPrefab = new LightPrefab("light", LightType.Directional, scene);
            lightPrefab.Transform.Translate(0, 10, 0);
            lightPrefab.Light.Range = 25;
            lightPrefab.Light.Intensity = 2.0f;
            lightPrefab.Light.FallOf = 5f;
            lightPrefab.Light.DiffuseColor = Color.LightCoral;
            lightPrefab.Light.Direction = new Vector3(0, 1, -1);
            lightPrefab.Light.Angle = MathHelper.PiOver4;
            lightPrefab.Light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            lightPrefab.AddComponent<LightMoverKeys>();
            lightPrefab.AddComponent<LightSwitcher>();

            var lightSphere = lightPrefab.AddComponent<MeshRenderer>();
            lightSphere.Geometry = new SphereGeometry(1f, 8);
            lightSphere.Geometry.Generate();
            lightSphere.CastShadow = false;
            lightSphere.ReceiveShadow = false;
            lightSphere.Material = new SimpleMaterial(scene);
            lightSphere.Material.MainTexture = GraphicsHelper.CreateTexture(Color.Yellow, 1, 1);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Content.Load<Texture2D>("Textures/terrainTexture");
            terrainMaterial.Shininess = 500;
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain", scene);
            terrain.Randomize();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);

            var water = new WaterPrefab("water", scene);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));
            scene.Destroy(water.Collider);

            // Cube
            var cubeSuperMaterial = new StandardMaterial(scene);
            cubeSuperMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.FloralWhite, Color.DodgerBlue); //Content.Load<Texture2D>("Textures/tech_box2");
            cubeSuperMaterial.DiffuseColor = Color.WhiteSmoke;
            cubeSuperMaterial.SpecularColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            cubeSuperMaterial.Shininess = 10;
            cubeSuperMaterial.EmissiveColor = new Color(0f, 0.0f, 0.1f, 1.0f);

            var cubeScene = new SceneObject();
            cubeScene.Name = "Super Cube";
            cubeScene.Transform.Translate(0, 5.5f, 15);
            cubeScene.Transform.LocalScale = new Vector3(1.0f);
            cubeScene.Transform.Rotate((float)Math.PI / 4, 0, (float)Math.PI / 4);
            var autoRot = cubeScene.AddComponent<AutoRotation>();
            autoRot.Rotation = new Vector3(0, 0.01f, 0);
            scene.Add(cubeScene);

            var cube = cubeScene.AddComponent<MeshRenderer>();
            cube.ReceiveShadow = false;
            cube.Geometry = new CubeGeometry();
            cube.Geometry.Generate();
            cube.Material = cubeSuperMaterial;
            cube.AddComponent<BoxCollider>();
            cube.AddComponent<BoundingBoxRenderer>();

            // Second cube
            var simpleMaterial = new SimpleMaterial(scene);
            simpleMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(new Color(1, 0, 0, 0.3f), new Color(1, 1, 1, 0.3f));
            simpleMaterial.Alpha = 0.3f;

            var cube2Scene = new SceneObject();
            cube2Scene.Name = "Simple Cube";
            cube2Scene.Transform.Translate(-20, 5.5f, -5);
            cube2Scene.Transform.LocalScale = new Vector3(3.0f);
            cube2Scene.Transform.Rotate(-MathHelper.PiOver4, 0, -MathHelper.PiOver4);
            var autoRot2 = cube2Scene.AddComponent<AutoRotation>();
            autoRot2.Rotation = new Vector3(0.02f, 0.01f, 0.03f);
            scene.Add(cube2Scene);

            var cube2 = cube2Scene.AddComponent<MeshRenderer>();
            cube2.ReceiveShadow = false;
            cube2.CastShadow = true;
            cube2.Geometry = new CubeGeometry();
            cube2.Geometry.Generate();
            cube2.Material = simpleMaterial;
            cube2.AddComponent<BoxCollider>();
            cube2.AddComponent<BoundingBoxRenderer>();

            var path = cube2.AddComponent<SimplePath>();
            path.Begin();
            path.AddPath(new Vector3(45, 3.5f, 45));
            path.AddPath(new Vector3(-45, 2.5f, 45));
            path.AddPath(new Vector3(-45, 5.5f, -45));
            path.AddPath(new Vector3(45, 5.5f, -45));
            path.End();

            // Third cube
            var reflectiveMaterial = new ReflectiveMaterial(scene);
            reflectiveMaterial.MainTexture = cubeSuperMaterial.MainTexture;
            reflectiveMaterial.TextureCube = renderer.Skybox.Texture;
            reflectiveMaterial.DiffuseColor = Color.Red;

            var cube3Scene = new SceneObject();
            cube3Scene.Name = "Reflective Cube";
            cube3Scene.Transform.Translate(0, 7.5f, -30);
            cube3Scene.Transform.LocalScale = new Vector3(4.0f, 2.0f, 1.5f);
            cube3Scene.Transform.Rotate(MathHelper.PiOver4, 0, -MathHelper.PiOver2);
            var autoRot3 = cube3Scene.AddComponent<AutoRotation>();
            autoRot3.Rotation = new Vector3(0.1f, 0.02f, 0.01f);
            scene.Add(cube3Scene);

            var cube3 = cube3Scene.AddComponent<MeshRenderer>();
            cube3.ReceiveShadow = false;
            cube3.CastShadow = true;
            cube3.Geometry = new CubeGeometry();
            cube3.Geometry.Generate();
            cube3.Material = reflectiveMaterial;
            cube3.AddComponent<SphereCollider>();
            cube3.AddComponent<BoundingBoxRenderer>();

            Screen.ShowCursor = true;
        }
    }
}
