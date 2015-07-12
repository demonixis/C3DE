using C3DE.Components;
using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace C3DE.Test
{
    [DataContract]
    public struct SerializedScene
    {
        [DataMember]
        public RenderSettings RenderSettings { get; set; }

        [DataMember]
        public SceneObject[] SceneObjects { get; set; }

        [DataMember]
        public Material[] Materials { get; set; }
    }

    public class TestApp : Scene
    {
        public static Type[] knowTypes;

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

            Screen.ShowCursor = true;
            RenderSettings.Skybox.Generate();
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.Exp2;
        }

        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.S))
            {
                Serialize("RenderSettings.xml", RenderSettings);

                var cubeGeometry = new CubeGeometry();
                cubeGeometry.Size = new Vector3(4, 3, 12);
                cubeGeometry.TextureRepeat = new Vector2(4, 1.5f);
                Serialize("CubeGeometry.xml", cubeGeometry);

                var sphereGeometry = new SphereGeometry();
                sphereGeometry.Size = new Vector3(5, 8, 9);
                sphereGeometry.TessellationLevel = 4;
                sphereGeometry.TextureRepeat = new Vector2(1, 0.5f);
                sphereGeometry.Radius = 45;
                Serialize("SphereGeometry.xml", sphereGeometry);

                var cylinderGeometry = new CylinderGeometry();
                Serialize("CylinderGeometry.xml", cylinderGeometry);

                var tr = new C3DE.Components.Transform();
                tr.Position = new Vector3(45, 12, 258);
                tr.Rotation = new Vector3(1, 1.55f, 0.98f);
                tr.LocalScale = new Vector3(1, 2, 10);
                Serialize("Transform.xml", tr);

                var cam = new Camera();
                Serialize("Camera.xml", cam);

                var material = new StandardMaterial(this);
                material.Texture = GraphicsHelper.CreateBorderTexture(Color.White, Color.Gray, 64, 64, 4);
                Serialize("StandardMaterial.xml", material);

                var sceneObject = new SceneObject("SO");
                var mr = sceneObject.AddComponent<MeshRenderer>();
                mr.Geometry = sphereGeometry;
                var bc = sceneObject.AddComponent<BoxCollider>();
                Serialize("SceneObject.xml", sceneObject);

                Serialize("Scene.xml", this);

                var s = new SerializedScene()
                {
                    Materials = materials.ToArray(),
                    SceneObjects = sceneObjects.ToArray(),
                    RenderSettings = RenderSettings
                };
                Serialize("SceneS.xml", s);

                /*
                foreach(var so in sceneObjects)
                    Serialize(so.Name + ".xml", so);

                Serialize("SceneObjects.xml", sceneObjects);*/
            }

            if (Input.Keys.JustPressed(Keys.L))
            {
                var cube = Deserialize("CubeGeometry.xml", typeof(CubeGeometry));
                var sphere = Deserialize("SphereGeometry.xml", typeof(SphereGeometry));
                var cylinder = Deserialize("CylinderGeometry.xml", typeof(CylinderGeometry));
                var tr = Deserialize("Transform.xml", typeof(Transform));
                var cam = Deserialize("Camera.xml", typeof(Camera));
                var mat = Deserialize("StandardMaterial.xml", typeof(StandardMaterial));
                var so = Deserialize("SceneObject.xml", typeof(SceneObject));
                var sc = Deserialize("Scene.xml", typeof(TestApp));
                var s = Deserialize("SceneS.xml", typeof(SerializedScene));
            }

            if (Input.Keys.Escape)
                Application.Quit();
        }

        private object Deserialize(string name, Type type)
        {
            GetKnowTypes();

            object result = null;

            var fileStream = new FileStream(name, FileMode.Open);
            var reader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
            var serializer = new DataContractSerializer(type, knowTypes);

            // Deserialize the data and read it from the instance.
            result = serializer.ReadObject(reader, true);
            reader.Close();
            fileStream.Close();

            return result;
        }

        private void Serialize(string name, object obj)
        {
            GetKnowTypes();

            var dataContractSerializer = new DataContractSerializer(obj.GetType(), knowTypes);
            var xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";

            using (var xmlWriter = XmlWriter.Create(name, xmlSettings))
            {
                dataContractSerializer.WriteObject(xmlWriter, obj);
                xmlWriter.Close();
            }
        }

        private void GetKnowTypes()
        {
            if (knowTypes == null)
            {
                var c3deTypes = GetDataContractTypes(typeof(SceneObject));
                var thisTypes = GetDataContractTypes(typeof(TestApp));
                var list = new List<Type>();
                list.AddRange(c3deTypes);
                list.AddRange(thisTypes);
                knowTypes = list.ToArray();
            }
        }

        private Type[] GetDataContractTypes(Type targetType)
        {
            var assembly = Assembly.GetAssembly(targetType);
            var types = assembly.GetTypes();
            var list = new List<Type>(types.Length);

            foreach (var type in types)
            {
                if (Attribute.IsDefined(type, typeof(DataContractAttribute)) || Attribute.IsDefined(type, typeof(CollectionDataContractAttribute)))
                    list.Add(type);
            }

            return list.ToArray();
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
