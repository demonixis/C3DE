using C3DE.Components;
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
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scenes
{
    public class PreLightingDemo : Scene
    {
        public PreLightingDemo() : base("Lighting demo") { }

        public override void Initialize()
        {
            base.Initialize();

            Application.Engine.Renderer = new PreLightRenderer();

            // Camera
            var camPrefab = new CameraPrefab("camera");
            var camera = camPrefab.GetComponent<Camera>();
            camera.Far = 50000;

            var orbit = camPrefab.AddComponent<OrbitController>();
            orbit.MaxDistance = 250;
            camPrefab.AddComponent<RayPickingTester>();
            Add(camPrefab);

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
            lightPrefab.AddComponent<DemoBehaviour>();

            // Terrain
            var terrainMaterial = new PreLightMaterial(scene);
            terrainMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128);
            //terrainMaterial.Shininess = 10;
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain");
            terrain.Renderer.Geometry.Size = new Vector3(4, 1, 4);
            terrain.Randomize(1, 2, 0.1, 0.2);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            for (int i = 0; i < 4; i++)
                AddObject();

            // Skybox
            RenderSettings.AmbientColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox, 1500.0f);

            Screen.ShowCursor = true;

            var v = 60;
            AddBackedLight(new Vector3(v, 15, v), Color.Red);
            AddBackedLight(new Vector3(-v, 15, v), Color.Blue);
            AddBackedLight(new Vector3(v, 15, -v), Color.Green);
            AddBackedLight(new Vector3(-v, 15, -v), Color.Yellow);  
        }

        public override void Unload()
        {
            Application.Engine.Renderer = new Renderer();
            base.Unload();
        }

        private void AddObject()
        {
            var sceneObject = new SceneObject();
            var cube = sceneObject.AddComponent<MeshRenderer>();
            cube.ReceiveShadow = false;
            cube.Geometry = new CubeGeometry();
            cube.Geometry.Generate();
            cube.Material = new PreLightMaterial(scene);
            cube.Material.MainTexture = GraphicsHelper.CreateCheckboardTexture(RandomHelper.GetColor(), RandomHelper.GetColor(), 64, 64);

            var inc = 75;
            var path = cube.AddComponent<SimplePath>();
            path.Loop = true;

            cube.Transform.Position = RandomHelper.GetVector3(-inc, 3, -inc, inc, 15, -inc);
            
            var autoRotate = cube.AddComponent<AutoRotation>();
            autoRotate.Rotation = Vector3.Normalize(RandomHelper.GetVector3(-0.01f, -0.01f, 0, 0.01f, 0.01f, 0));

            path.Begin();

            for (int i = 0; i < 6; i++)
                path.AddPath(RandomHelper.GetVector3(-inc, 0, -inc, inc, 0, inc), cube.Transform);

            path.End();

            Add(sceneObject);
        }

        private void AddBackedLight(Vector3 position, Color color)
        {
            var sceneObject = new SceneObject("Light_" + color.ToString());
            sceneObject.Transform.Position = position;

            var light = sceneObject.AddComponent<Light>();
            light.FallOf = 10f;
            light.Intensity = 1.5f;
            light.Range = 65;
            light.DiffuseColor = color;
            light.Backing = LightRenderMode.Backed;

            Add(sceneObject);
        }
    }
}
