using C3DE.Components.Colliders;
using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scenes
{
    public class LightingDemo : Scene
    {
        public LightingDemo() : base("Lighting demo") { }

        public override void Initialize()
        {
            base.Initialize();

            // Camera
            var camera = new CameraPrefab("camera");
            camera.AddComponent<OrbitController>();
            camera.AddComponent<RayPickingTester>();
            Add(camera);
            
            // Light
            var lightPrefab = new LightPrefab("lightPrefab", LightType.Point);
            Add(lightPrefab);
            lightPrefab.Transform.Position = new Vector3(0, 15, 15);
            lightPrefab.Light.Range = 105;
            lightPrefab.Light.Intensity = 2.0f;
            lightPrefab.Light.FallOf = 5f;
            lightPrefab.Light.DiffuseColor = Color.Violet;
            lightPrefab.Light.Direction = new Vector3(-1, 1, -1);
            lightPrefab.Light.Angle = 0.1f;
            lightPrefab.Light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            lightPrefab.AddComponent<LightSwitcher>();
            lightPrefab.AddComponent<LightMover>();
            lightPrefab.AddComponent<DemoBehaviour>();

            var lightPrefabSphere = lightPrefab.AddComponent<MeshRenderer>();
            lightPrefabSphere.Geometry = new SphereGeometry(2f, 4);
            lightPrefabSphere.Geometry.Generate();
            lightPrefabSphere.CastShadow = false;
            lightPrefabSphere.ReceiveShadow = false;
            lightPrefabSphere.Material = new SimpleMaterial(scene);
            lightPrefabSphere.Material.MainTexture = GraphicsHelper.CreateTexture(Color.Yellow, 1, 1);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = GraphicsHelper.CreateBorderTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128, 1);
            terrainMaterial.Shininess = 10;
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain");
            terrain.Flat();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            // Cube
            var cubeSuperMaterial = new StandardMaterial(scene);
            cubeSuperMaterial.MainTexture = GraphicsHelper.CreateTriangleTexture(Color.Red, Color.White);  //GraphicsHelper.CreateCheckboardTexture(Color.FloralWhite, Color.DodgerBlue); //Content.Load<Texture2D>("Textures/tech_box2");
            cubeSuperMaterial.DiffuseColor = Color.WhiteSmoke;
            cubeSuperMaterial.SpecularColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            cubeSuperMaterial.Shininess = 10;
            cubeSuperMaterial.EmissiveColor = new Color(0f, 0.0f, 0.2f, 1.0f);

            var cubeScene = new SceneObject();
            cubeScene.Transform.Translate(0, 6f, 0);
            cubeScene.Transform.LocalScale = new Vector3(4.0f);
            cubeScene.Transform.Rotate((float)Math.PI / 4, 0, (float)Math.PI / 4);
            var autoRot = cubeScene.AddComponent<AutoRotation>();
            autoRot.Rotation = new Vector3(0, 0.01f, 0);
            Add(cubeScene);

            var cube = cubeScene.AddComponent<MeshRenderer>();
            cube.ReceiveShadow = false;
            cube.Geometry = new CubeGeometry();
            cube.Geometry.Generate();
            cube.Material = cubeSuperMaterial;

            cubeScene.AddComponent<BoxCollider>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            Screen.ShowCursor = true;
        }
    }
}
