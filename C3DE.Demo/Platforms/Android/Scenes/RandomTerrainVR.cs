using System;
using C3DE.Components.Colliders;
using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Prefabs.Meshes;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Demo.Scripts;

namespace C3DE.Demo.Scenes
{
    public class RandomTerrainVR : Scene
    {
        public RandomTerrainVR() : base("Random Terrain VR") { }  

        public override void Initialize()
        {
            base.Initialize();

            //(Application.Game as AndroidGame).Renderer = new VRRenderer();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // Camera
            var camera = new CameraPrefab("camera");
            camera.AddComponent<DeviceOrientationController> ();
            camera.AddComponent<DemoBehaviour>();
            camera.Transform.Position = new Vector3 (0, -10, -15);
            Add(camera);

            // Light
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            lightPrefab.Transform.Translate(0, 10, 0);
            Add(lightPrefab);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain");
            terrain.Randomize();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            var water = new WaterPrefab("water");
            Add(water);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));
            (water.Renderer.Material as WaterMaterial).WaterTransparency = 0.6f;

            var cube = new MeshPrefab<CubeGeometry>();
            cube.Transform.Translate(0, 6, 0);
            cube.Transform.LocalScale = new Vector3(2.5f);
            cube.Renderer.Material = new StandardMaterial(scene);
            cube.Renderer.Material.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.Blue, Color.AntiqueWhite, 16, 16);
            Add(cube);

            var autoRot = cube.AddComponent<AutoRotation>();
            autoRot.Rotation = new Vector3(0.075f, 0.075f, 0.0f);
        }
    }
}
