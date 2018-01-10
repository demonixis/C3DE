using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Demo.Scripts;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
    public class SimpleDemo : Scene
    {
        protected Camera m_Camera;
        protected Light m_DirectionalLight;
        protected ControllerSwitcher m_ControllerSwitcher;
        protected DemoSceneMenu m_DemoSceneMenu;

        public SimpleDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            GUI.Skin = DemoGame.CreateSkin(Application.Content, false);

            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            m_Camera = camera.GetComponent<Camera>();
            m_Camera.AddComponent<DemoBehaviour>();
            m_ControllerSwitcher = m_Camera.AddComponent<ControllerSwitcher>();
            m_DemoSceneMenu = m_Camera.AddComponent<DemoSceneMenu>();

            // And a light
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1f);
            lightGo.Transform.LocalPosition = new Vector3(250, 500, 100);
            lightGo.Transform.LocalRotation = new Vector3(1, 0.5f, 0);
            m_DirectionalLight = lightGo.GetComponent<Light>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // Fog: Setup fog mode with some value. It's still disabled, but those values are used by the post processing fog effect.
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.None;
            RenderSettings.FogColor = Color.FloralWhite;
        }

        public void OptimizeFor2D()
        {
            Destroy(m_DirectionalLight);
            Destroy(m_DemoSceneMenu);
            RenderSettings.Skybox.Enabled = false;
        }

        public void SetControlMode(ControllerSwitcher.ControllerType type, Vector3 position, Vector3 rotation, bool fly = true)
        {
            m_ControllerSwitcher.DefaultPosition = position;
            m_ControllerSwitcher.DefaultRotation = rotation;
            m_ControllerSwitcher.FlyMode = fly;
            m_ControllerSwitcher.SetControllerActive(type);
        }
    }
}
