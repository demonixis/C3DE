using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Demo.Scripts;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo
{
    public class TerrainDemo : Engine
    {
        public TerrainDemo()
            : base()
        {
            Window.Title = "C3DE - Terrain";
            graphics.PreferredBackBufferWidth = Demo.ScreenWidth;
            graphics.PreferredBackBufferHeight = Demo.ScreenHeight;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();

            var scene = sceneManager.ActiveScene;

            // Add a camera with a FPS controller
            var camera = new CameraPrefab("camera");
            scene.Add(camera);
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            var cc = camera.AddComponent<ControllerSwitcher>();
            cc.SetControllerActive(1);
            
            // And a light
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            scene.Add(lightPrefab);
            lightPrefab.Light.Direction = new Vector3(1, 1, 0);
            lightPrefab.Light.DiffuseColor = Color.LightSkyBlue;
            lightPrefab.Light.Intensity = 1.5f;
            lightPrefab.AddComponent<LightMoverKeys>();
            lightPrefab.AddComponent<LightSwitcher>();
            lightPrefab.AddComponent<DemoBehaviour>();
            lightPrefab.EnableShadows = true;

            // Finally a terrain
            var terrainMat = new TerrainMaterial(scene);

            var terrain = new TerrainPrefab("terrain");
            scene.Add(terrain);
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMat;
            terrain.Transform.Translate(-terrain.Width >> 1, -10, -terrain.Depth >> 1);
            terrain.AddComponent<WeightMapViewer>();
            var map = terrain.GenerateWeightMap();

            terrainMat.MainTexture = Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMat.SandTexture = Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMat.SnowTexture = Content.Load<Texture2D>("Textures/Terrain/Snow");
            terrainMat.RockTexture = Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMat.WeightTexture = map;
            terrainMat.Tiling = new Vector2(4);
            
            // With water !
            var water = new WaterPrefab("water");
            scene.Add(water);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));

            // Don't miss the Skybox ;)
            renderer.Skybox.Generate(GraphicsDevice, Content, Demo.BlueSkybox);

            Input.Gamepad.Sensitivity = new Vector2(1, 0.75f);
            Screen.ShowCursor = true;
            GUI.Skin = Demo.CreateSkin(Content);

            scene.RenderSettings.FogDensity = 0.01f;
            scene.RenderSettings.FogMode = FogMode.Exp2;
        }
    }
}
