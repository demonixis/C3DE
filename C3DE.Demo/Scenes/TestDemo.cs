using C3DE.Components.Controllers;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Extensions;
using C3DE.Graphics.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class TestDemo : Scene
    {
        public TestDemo() : base("Test") { }

        public override void Initialize()
        {
            base.Initialize();

            // Camera
            var cameraGo = GameObjectFactory.CreateCamera();
            cameraGo.AddComponent<OrbitController>();
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);

            // Light
            var lightGo = GameObjectFactory.CreateLight(LightType.Point, Color.White, 1, 0);
            lightGo.Transform.Position = new Vector3(0, 15, 15);
            lightGo.Transform.Rotation = new Vector3(-1, 1, 0);
            Add(lightGo);

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // Model
            var model = Application.Content.Load<Model>("Models/Jack/JackOLantern");
            var mesh = model.ToMeshRenderers(this);
            Add(mesh);

            Screen.ShowCursor = true;
        }
    }
}
