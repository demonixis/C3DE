using C3DE.Components;
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

namespace C3DE.Demo.Scenes
{
    public class PreLightingDemo : Scene
    {
        private Rendering.Renderer _prevRenderer;

        public PreLightingDemo() : base("PrelLighting Renderer") { }

        public override void Initialize()
        {
            base.Initialize();

            _prevRenderer = Application.Engine.Renderer;

            Application.Engine.Renderer = new PreLightRenderer(Application.GraphicsDevice);

            // Camera
            var camPrefab = new CameraPrefab("camera");
            camPrefab.AddComponent<DemoBehaviour>();
            var camera = camPrefab.GetComponent<Camera>();
            camera.Far = 50000;

            var orbit = camPrefab.AddComponent<OrbitController>();
            orbit.MaxDistance = 250;
            Add(camPrefab);

            // Terrain
            var groundMaterial = new PreLightMaterial(scene);
            groundMaterial.Texture = GraphicsHelper.CreateCheckboardTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128);
            groundMaterial.Tiling = new Vector2(16);

            var ground = new TerrainPrefab("terrain");
            ground.Renderer.Geometry.Size = new Vector3(4, 1, 4);
            ground.Flatten();
            ground.Renderer.Material = groundMaterial;
            ground.Transform.Translate(-ground.Width >> 1, 0, -ground.Depth / 2);
            Add(ground);

            MeshRenderer cube = null;
            var maxCubes = 24;
            var limits = 250;
            for (int i = 0; i < maxCubes; i++)
            {
                var size = RandomHelper.GetVector3(0.5f, 0.5f, 0.5f, 16, 16, 16);
                cube = CreateCube(size, RandomHelper.GetVector3(-limits, size.Y, -limits, limits, size.Y, limits));
                cube.Material = new PreLightMaterial(this);
                cube.Material.Texture = GraphicsHelper.CreateCheckboardTexture(RandomHelper.GetColor(), RandomHelper.GetColor());
                Add(cube.SceneObject);
            }

            for (int i = 0; i < 4; i++)
                AddObject();

            // Skybox
            RenderSettings.AmbientColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox, 1500.0f);

            Screen.ShowCursor = true;

            var margin = 80;
            AddLight(new Vector3(0, 15, 0), Color.DarkCyan, 35);
            AddLight(new Vector3(margin, 15, margin), Color.Red);
            AddLight(new Vector3(-margin, 15, margin), Color.Blue);
            AddLight(new Vector3(margin, 15, -margin), Color.Green);
            AddLight(new Vector3(-margin, 15, -margin), Color.Yellow);
        }

        public override void Unload()
        {
            Application.Engine.Renderer = _prevRenderer;
            base.Unload();
        }

        private void AddObject()
        {
            var sceneObject = new GameObject();
            var cube = sceneObject.AddComponent<MeshRenderer>();
            cube.ReceiveShadow = false;
            cube.Geometry = new CubeGeometry();
            cube.Geometry.Build();
            cube.Material = new PreLightMaterial(scene);
            cube.Material.Texture = GraphicsHelper.CreateCheckboardTexture(RandomHelper.GetColor(), RandomHelper.GetColor(), 64, 64);

            var inc = 75;
            var path = cube.AddComponent<SimplePath>();
            path.Loop = true;

            cube.Transform.Position = RandomHelper.GetVector3(-inc, 3, -inc, inc, 15, -inc);
            cube.Transform.LocalScale = new Vector3(4);

            var autoRotate = cube.AddComponent<AutoRotation>();
            autoRotate.Rotation = Vector3.Normalize(RandomHelper.GetVector3(-0.01f, -0.01f, 0, 0.01f, 0.01f, 0));

            path.Begin();

            for (int i = 0; i < 6; i++)
                path.AddPath(RandomHelper.GetVector3(-inc, 0, -inc, inc, 0, inc), cube.Transform);

            path.End();

            Add(sceneObject);
        }

        private MeshRenderer CreateCube(Vector3 size, Vector3 position)
        {
            var sceneObject = new GameObject("Cube");
            sceneObject.Transform.Position = position;

            var renderer = sceneObject.AddComponent<MeshRenderer>();
            renderer.Geometry = new CubeGeometry();
            renderer.Geometry.Size = size;
            renderer.Geometry.Build();

            return renderer;
        }

        private void AddLight(Vector3 position, Color color, float range = 65f)
        {
            var sceneObject = new GameObject("Light_" + color.ToString());
            sceneObject.Transform.Position = position;

            var light = sceneObject.AddComponent<Light>();
            light.FallOf = 10f;
            light.Intensity = 1.5f;
            light.Range = range;
            light.Color = color;
            light.Backing = LightRenderMode.Backed;

            Add(sceneObject);
        }
    }
}
