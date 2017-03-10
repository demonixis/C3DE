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
using Microsoft.Xna.Framework.Graphics;
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

            var vrDevice = new OSVRService(Application.Engine);
            var vrRenderer = new VRRenderer(Application.GraphicsDevice, vrDevice);
            vrRenderer.StereoPreview = true;
            Application.Engine.Renderer = vrRenderer;
        }

        public override void Unload()
        {
            Application.Engine.Renderer = _prevRenderer;
            base.Unload();
        }

        private void BuildScene()
        {
            // Add a camera with a FPS controller
            var camera = new CameraPrefab("camera");
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            camera.AddComponent<ControllerSwitcher>();
            Add(camera);

            // And a light
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            Add(lightPrefab);
            lightPrefab.Transform.Rotation = new Vector3(-1, 1, 0);
            lightPrefab.Light.Color = Color.LightSkyBlue;
            lightPrefab.Light.Intensity = 1.0f;
            lightPrefab.AddComponent<DemoBehaviour>();
            lightPrefab.EnableShadows = true;

            // Finally a terrain
            var terrainMat = new TerrainMaterial(scene);

            var terrain = new TerrainPrefab("terrain");
            scene.Add(terrain);
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMat;
            terrain.Transform.Translate(-terrain.Width >> 1, -10, -terrain.Depth >> 1);

            var map = terrain.GenerateWeightMap();
            terrainMat.Texture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMat.SandTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMat.SnowTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Snow");
            terrainMat.RockTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMat.WeightTexture = map;
            terrainMat.Tiling = new Vector2(4);

            // With water !
            var water = new WaterPrefab("water");
            scene.Add(water);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));

            Screen.ShowCursor = true;

            // Don't miss the Skybox ;)
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.Exp2;
        }
    }
}