using C3DE.Components.Colliders;
using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Rendering;
using C3DE.Utils;
using C3DE.VR;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scenes
{
    public class VirtualRealityDemo : Scene
    {
        private Rendering.Renderer _prevRenderer;

        public VirtualRealityDemo() : base("Virtual Reality demo") { }

        public override void Initialize()
        {
            base.Initialize();

            BuildScene();

            _prevRenderer = Application.Engine.Renderer;

            var vrDevice = new OpenVRService(Application.Engine);
            //var vrDevice = new NullVRService(Application.Engine);
            //var vrDevice = new OculusRiftService(Application.Engine);
            var vrRenderer = new VRRenderer(Application.GraphicsDevice, vrDevice);
            //vrRenderer.StereoPreview = true;
            Application.Engine.Renderer = vrRenderer;
        }

        public override void Unload()
        {
            Application.Engine.Renderer = _prevRenderer;
            base.Unload();
        }

        private void BuildScene()
        {
            // Camera
            var camera = new CameraPrefab("camera");
            camera.AddComponent<OrbitController>();
            camera.AddComponent<RayPickingTester>();
            Add(camera);

            // Light
            var lightPrefab = new LightPrefab("lightPrefab", LightType.Point);
            Add(lightPrefab);
            lightPrefab.AddComponent<LightSwitcher>();
            lightPrefab.AddComponent<DemoBehaviour>();

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.Texture = GraphicsHelper.CreateBorderTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128, 4);
            terrainMaterial.Shininess = 10;
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain");
            terrain.Renderer.Geometry.Size = new Vector3(4);
            terrain.Renderer.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

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