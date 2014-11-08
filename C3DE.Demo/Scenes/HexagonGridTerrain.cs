using C3DE.Components.Colliders;
using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Prefabs.Meshes;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scenes
{
    public class HexagonTerrainDemo : Scene
    {
        public HexagonTerrainDemo() : base("Hexagonal Grid") { }

        public override void Initialize()
        {
            base.Initialize();

            // Camera
            var camera = new CameraPrefab("camera");
            camera.AddComponent<OrbitController>();
            camera.AddComponent<DemoBehaviour>();
            camera.AddComponent<PostProcessSwitcher>();
            Add(camera);

            // Light
            var lightPrefab = new LightPrefab("lightPrefab", LightType.Directional);
            Add(lightPrefab);

            var sceneObject = new SceneObject("HexaGrid");
            sceneObject.AddComponent<HexaGridBuilder>();
            Add(sceneObject);

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            Screen.ShowCursor = true;
        }
    }
}
