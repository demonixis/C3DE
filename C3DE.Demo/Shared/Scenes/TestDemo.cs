using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Components.Lighting;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Graphics.Rendering;
using C3DE.Graphics;

namespace C3DE.Demo.Scenes
{
    public class TestDemo : SimpleDemo
    {
        public TestDemo() : base("Test")
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var lightGo = GameObjectFactory.CreateLight(LightType.Point, Color.Red, 10f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(0, 10, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);

            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            var _camera = camera.GetComponent<Camera>();
            _camera.AddComponent<DemoBehaviour>();
            var _controllerSwitcher = _camera.AddComponent<ControllerSwitcher>();

            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.NatureSkybox, 256);

            var reflectionProbe = new GameObject("ReflectionProbe");
            reflectionProbe.Transform.Position = new Vector3(0, 25, 0);
            var probe = reflectionProbe.AddComponent<ReflectionProbe>();
            probe.Size = 1024;
            probe.Mode = ReflectionProbe.RenderingMode.Realtime;
            //reflectionProbe.AddComponent<ReflectionProbeViewer>();

            var content = Application.Content;
            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = TextureFactory.CreateCheckboard(Color.Black, Color.White);
            terrainMaterial.ReflectionIntensity = 2.95f;
            terrainMaterial.ReflectionMap = probe.ReflectionMap;
            terrainMaterial.Tiling = new Vector2(16);
            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;


            for (var i = 2; i < 12; i += 2)
            {
                AddMesh(new Vector3(i, 2, i), probe);
                AddMesh(new Vector3(i, 2, -i), probe);
                AddMesh(new Vector3(-i, 2, i), probe);
                AddMesh(new Vector3(-i, 2, -i), probe);
            }

            AddLightGroundTest();
        }

        private void AddMesh(Vector3 pos, ReflectionProbe probe)
        {
            var mesh = GameObjectFactory.CreateMesh(GeometryType.Cube);
            mesh.GetComponent<Renderer>().Material = new StandardMaterial
            {
                MainTexture = TextureFactory.CreateCheckboard(Color.Red, Color.AntiqueWhite),
                ReflectionIntensity = 2.95f,
                ReflectionMap = probe.ReflectionMap
            };
            mesh.Transform.LocalPosition = pos;
        }
    }
}
