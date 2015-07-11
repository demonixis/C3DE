using C3DE.Components;
using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace C3DE.Test
{
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
            sceneObjects.Add(lightPrefab);

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

            /*
            foreach(var so in sceneObjects)
                Serialize(so.Name + ".xml", so);

            Serialize("SceneObjects.xml", sceneObjects);*/

            Application.Quit();
        }

        private void Serialize(string name, object obj)
        {
            var dataContractSerializer = new DataContractSerializer(obj.GetType());
            var xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";

            using (var xmlWriter = XmlWriter.Create(name, xmlSettings))
            {
                dataContractSerializer.WriteObject(xmlWriter, obj);
                xmlWriter.Close();
            }
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
