using C3DE.Components.Lights;
using C3DE.Materials;
using C3DE.Prefabs;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Serialization;

namespace C3DE.Test
{
    public class Item
    {
        [XmlAttribute]
        public int Id;
        [XmlAttribute]
        public object Value;
    }

    class TestApp : Scene
    {
        public TestApp()
            : base("TestScene")
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            // Add a camera with a FPS controller
            var camera = new CameraPrefab("camera");
            camera.Setup(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);
            Add(camera);

            // And a light
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            Add(lightPrefab);
            lightPrefab.Light.Direction = new Vector3(1, 1, 0);
            lightPrefab.Light.DiffuseColor = Color.LightSkyBlue;
            lightPrefab.Light.Intensity = 1.5f;
            lightPrefab.EnableShadows = true;

            // Finally a terrain
            var terrainMat = new TerrainMaterial(scene);

            var terrain = new TerrainPrefab("terrain");
            scene.Add(terrain);
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMat;
            terrain.Transform.Translate(-terrain.Width >> 1, -10, -terrain.Depth >> 1);
            var map = terrain.GenerateWeightMap();

            terrainMat.WeightTexture = map;
            terrainMat.Tiling = new Vector2(4);

            // With water !
            var water = new WaterPrefab("water");
            scene.Add(water);

            Screen.ShowCursor = true;

            // Don't miss the Skybox ;)
            RenderSettings.Skybox.Generate();

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.Exp2;
        }

        public override void Update()
        {
            base.Update();


            Application.Quit();
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var game = new Engine("C3DE Test", 800, 600))
            {
                Application.SceneManager.Add(new TestApp());
                Application.LoadLevel(0);
                game.Run();
            }
        }
    }
}
