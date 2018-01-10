using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Demo.Scripts;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class SimpleDemo : Scene
    {
        protected Camera m_Camera;
        protected Light m_DirectionalLight;

        public SimpleDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            m_Camera = camera.GetComponent<Camera>();
            m_Camera.AddComponent<DemoBehaviour>();
            m_Camera.AddComponent<DemoMenu>();
            m_Camera.AddComponent<ControllerSwitcher>();

            // And a light
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1f);
            lightGo.Transform.LocalPosition = new Vector3(250, 500, 100);
            lightGo.Transform.LocalRotation = new Vector3(1, 0.5f, 0);
            m_DirectionalLight = lightGo.GetComponent<Light>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);
        }
    }
}
