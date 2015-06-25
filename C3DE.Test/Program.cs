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

            var camera = new CameraPrefab("cam");
            Add(camera);

            var material = new SimpleMaterial(this);
            material.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.Blue, Color.Red);

            //XMLSerialization("camera.xml", camera);
            JSONSerialization("camera.json", camera);

            //XMLSerialization("material.xml", material);
            JSONSerialization("material.json", material);

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
