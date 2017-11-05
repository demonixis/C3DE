using C3DE.Components.Physics;
using C3DE.Components.Controllers;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Geometries;
using C3DE.Graphics.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scenes
{
    public class LightingDemo : Scene
    {
        public LightingDemo() : base("Realtime Lighting") { }

        public override void Initialize()
        {
            base.Initialize();

            // Camera
            var cameraGo = GameObjectFactory.CreateCamera();
            cameraGo.AddComponent<OrbitController>();
            cameraGo.AddComponent<RayPickingTester>();
            Add(cameraGo);

            // Light
            var lightGo = GameObjectFactory.CreateLight(LightType.Point);
            lightGo.Transform.Position = new Vector3(0, 15, 15);
            lightGo.Transform.Rotation = new Vector3(-1, 1, 0);
            Add(lightGo);

            var light = lightGo.GetComponent<Light>();
            light.Range = 105;
            light.Intensity = 2.0f;
            light.FallOf = 5f;
            light.Color = Color.Violet;
            light.Angle = 0.1f;
            light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
            light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);

            lightGo.AddComponent<LightSwitcher>();
            lightGo.AddComponent<LightMover>();
            lightGo.AddComponent<DemoBehaviour>();

            var ligthSphere = lightGo.AddComponent<MeshRenderer>();
            ligthSphere.Geometry = new SphereGeometry(2f, 4);
            ligthSphere.Geometry.Build();
            ligthSphere.CastShadow = false;
            ligthSphere.ReceiveShadow = false;
            ligthSphere.Material = new SimpleMaterial(scene);
            ligthSphere.Material.Texture = GraphicsHelper.CreateTexture(Color.Yellow, 1, 1);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.Texture = GraphicsHelper.CreateBorderTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128, 4);
            terrainMaterial.Shininess = 10;
            terrainMaterial.Tiling = new Vector2(16);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(4);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrainGo.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrainGo);

            // Cube
            var cubeSuperMaterial = new StandardMaterial(scene);
            cubeSuperMaterial.Texture = GraphicsHelper.CreateCheckboardTexture(Color.FloralWhite, Color.DodgerBlue);
            cubeSuperMaterial.DiffuseColor = Color.WhiteSmoke;
            cubeSuperMaterial.SpecularColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
            cubeSuperMaterial.Shininess = 10;
            cubeSuperMaterial.EmissiveColor = new Color(0f, 0.0f, 0.2f, 1.0f);

            var cubeScene = new GameObject();
            cubeScene.Transform.Translate(0, 6f, 0);
            cubeScene.Transform.LocalScale = new Vector3(4.0f);
            cubeScene.Transform.Rotate((float)Math.PI / 4, 0, (float)Math.PI / 4);
            var autoRot = cubeScene.AddComponent<AutoRotation>();
            autoRot.Rotation = new Vector3(0, 0.01f, 0);
            Add(cubeScene);

            var cube = cubeScene.AddComponent<MeshRenderer>();
            cube.ReceiveShadow = false;
            cube.Geometry = new CubeGeometry();
            cube.Geometry.Build();
            cube.Material = cubeSuperMaterial;

            cubeScene.AddComponent<BoxCollider>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox, 500);

            Screen.ShowCursor = true;
        }
    }
}