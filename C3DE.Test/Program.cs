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
            Serializr.AddTypes(typeof(TestApp));
        }

        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.S))
            {
                Serializr.Serialize("RenderSettings.xml", RenderSettings);

                var cubeGeometry = new CubeGeometry();
                cubeGeometry.Size = new Vector3(4, 3, 12);
                cubeGeometry.TextureRepeat = new Vector2(4, 1.5f);
                Serializr.Serialize("CubeGeometry.xml", cubeGeometry);

                var sphereGeometry = new SphereGeometry();
                sphereGeometry.Size = new Vector3(5, 8, 9);
                sphereGeometry.TessellationLevel = 4;
                sphereGeometry.TextureRepeat = new Vector2(1, 0.5f);
                sphereGeometry.Radius = 45;
                Serializr.Serialize("SphereGeometry.xml", sphereGeometry);

                var cylinderGeometry = new CylinderGeometry();
                Serializr.Serialize("CylinderGeometry.xml", cylinderGeometry);

                var tr = new C3DE.Components.Transform();
                tr.Position = new Vector3(45, 12, 258);
                tr.Rotation = new Vector3(1, 1.55f, 0.98f);
                tr.LocalScale = new Vector3(1, 2, 10);
                Serializr.Serialize("Transform.xml", tr);

                var cam = new Camera();
                Serializr.Serialize("Camera.xml", cam);

                var material = new StandardMaterial(this);
                material.Texture = GraphicsHelper.CreateBorderTexture(Color.White, Color.Gray, 64, 64, 4);
                Serializr.Serialize("StandardMaterial.xml", material);

                var sceneObject = new SceneObject("SO");
                var mr = sceneObject.AddComponent<MeshRenderer>();
                mr.Geometry = sphereGeometry;
                var bc = sceneObject.AddComponent<BoxCollider>();
                Serializr.Serialize("SceneObject.xml", sceneObject);

                Serializr.Serialize("Scene.xml", this);

                var s = new SerializedScene()
                {
                    Materials = materials.ToArray(),
                    SceneObjects = sceneObjects.ToArray(),
                    RenderSettings = RenderSettings
                };
                Serializr.Serialize("SceneS.xml", s);
            }

            if (Input.Keys.JustPressed(Keys.L))
            {
                var cube = Serializr.Deserialize("CubeGeometry.xml", typeof(CubeGeometry));
                var sphere = Serializr.Deserialize("SphereGeometry.xml", typeof(SphereGeometry));
                var cylinder = Serializr.Deserialize("CylinderGeometry.xml", typeof(CylinderGeometry));
                var tr = Serializr.Deserialize("Transform.xml", typeof(Transform));
                var cam = Serializr.Deserialize("Camera.xml", typeof(Camera));
                var mat = Serializr.Deserialize("StandardMaterial.xml", typeof(StandardMaterial));
                var so = Serializr.Deserialize("SceneObject.xml", typeof(SceneObject));
                var sc = Serializr.Deserialize("Scene.xml", typeof(TestApp));
                var s = Serializr.Deserialize("SceneS.xml", typeof(SerializedScene));
            }

            if (Input.Keys.Escape)
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
