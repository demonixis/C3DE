using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using C3DE.Components.Lights;

namespace C3DE.Test
{
    class TestApp : Scene
    {
        public TestApp ()
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

            //XMLSerialization("camera.xml", camera);
            JSONSerialization("camera.json", camera);

            //XMLSerialization("ligth.xml", lightPrefab);
            JSONSerialization("ligth.json", lightPrefab);

            //XMLSerialization("material.xml", material);
            JSONSerialization("material.json", terrainMat);
        }

        public override void Update()
        {
            base.Update();

            JSONSerialization("scene.json", this);

            Application.Quit();
        }

        private void XMLSerialization(string path, ISerializable obj)
        {
            var data = obj.Serialize();
            var ser = new XmlSerializer(data.GetType());
            using (var w = new StreamWriter(path))
            {
                ser.Serialize(w, data);
                w.Close();
            }
        }

        private void JSONSerialization(string path, ISerializable obj)
        {
            var d = obj.Serialize();
            var data = JsonConvert.SerializeObject(d, Formatting.Indented);
            File.WriteAllText(path, data);
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
