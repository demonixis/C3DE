using C3DE.Components.Lights;
using C3DE.Demo.Kinect.Scripts;
using C3DE.Demo.Scripts;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Kinect.Scenes
{
    public class KinectTerrainDemo : Scene
    {
        public KinectTerrainDemo() : base("Kinect demo") { }

        public override void Initialize()
        {
            base.Initialize();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, KinectGame.BlueSkybox);

            // Camera
            var camera = new CameraPrefab("camera");
            camera.AddComponent<KinectCameraController>();
            Add(camera);

            // Light
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            lightPrefab.Transform.Translate(0, 10, 0);
            lightPrefab.Light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            Add(lightPrefab);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/terrainTexture");
            terrainMaterial.Shininess = 300;
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain");
            terrain.Randomize(2, 5, 0.09, 0.01);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            var water = new WaterPrefab("water");
            Add(water);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));
            (water.Renderer.Material as WaterMaterial).ReflectiveMap = scene.RenderSettings.Skybox.Texture;
            (water.Renderer.Material as WaterMaterial).WaterTransparency = 0.6f;

            GUI.Skin = KinectGame.CreateSkin(Application.Content);
        }
    }
}
