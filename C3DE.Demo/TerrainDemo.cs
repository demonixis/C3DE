using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.Components.Lights;
using C3DE.Demo.Scripts;
using C3DE.Materials;
using C3DE.Prefabs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo
{
    public class TerrainDemo : Engine
    {
        Transform lightTransform;

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

            // Add a camera with a FPS controller
            var camera = new CameraPrefab("camera", scene);
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            var cc = camera.AddComponent<ControllerSwitcher>();
            cc.SetControllerActive(1);

            // And a light
            var lightPrefab = new LightPrefab("light", LightType.Directional, scene);
            lightPrefab.Light.Direction = new Vector3(1, 1, 0);
            lightPrefab.Light.DiffuseColor = Color.LightSkyBlue;
            lightPrefab.AddComponent<LightMoverKeys>();
            lightPrefab.AddComponent<LightSwitcher>();
            lightPrefab.AddComponent<DemoBehaviour>();
            lightPrefab.EnableShadows = true;

            // Finally a terrain
            var terrainMat = new StandardMaterial(scene);
            terrainMat.MainTexture = Content.Load<Texture2D>("Textures/terrainTexture");
            terrainMat.Shininess = 50;

            var terrain = new TerrainPrefab("terrain", scene);
            terrain.TextureRepeat = new Vector2(2);
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMat;
            terrain.Transform.Translate(-terrain.Width >> 1, -10, -terrain.Depth >> 1);            

            // With water !
            var water = new WaterPrefab("water", scene);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));
            
            // Don't miss the Skybox ;)
            renderer.Skybox.Generate(GraphicsDevice, Content, Demo.BlueSkybox);

            Input.Gamepad.Sensitivity = new Vector2(1, 0.75f);
            Screen.ShowCursor = true;

            scene.RenderSettings.FogMode = FogMode.Linear;
        }
    }
}
