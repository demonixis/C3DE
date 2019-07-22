using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scenes
{
    public class SimpleDemo : Scene
    {
        protected static Color[] ValidColors = new[]
        {
            Color.Red, Color.Green, Color.Blue,
            Color.Purple, Color.Cyan, Color.Yellow
        };

        protected Camera _camera;
        protected Light _directionalLight;
        protected ControllerSwitcher _controllerSwitcher;
        protected DemoSceneMenu _demoSceneMenu;

        public SimpleDemo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            GUI.Skin = DemoGame.CreateSkin(Application.Content, false);

            SceneSetup();
        }

        protected virtual void SceneSetup()
        {
            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            _camera = camera.GetComponent<Camera>();
            _camera.AddComponent<DemoBehaviour>();
            _controllerSwitcher = _camera.AddComponent<ControllerSwitcher>();
            _demoSceneMenu = _camera.AddComponent<DemoSceneMenu>();

            // And a light
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1f, 2048);
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);
            _directionalLight = lightGo.GetComponent<Light>();

#if WINDOWS
            // Sun Flares
            var content = Application.Content;
            var glowTexture = content.Load<Texture2D>("Textures/Flares/SunGlow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/circle"),
                content.Load<Texture2D>("Textures/Flares/circle_sharp_1"),
                content.Load<Texture2D>("Textures/Flares/circle_soft_1")
            };

            var sunflares = _directionalLight.AddComponent<LensFlare>();
            sunflares.Setup(glowTexture, flareTextures);
#endif

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.BlueSkybox);

            // Fog: Setup fog mode with some value. It's still disabled, but those values are used by the post processing fog effect.
            //RenderSettings.FogDensity = 0.0085f;
            //RenderSettings.FogMode = FogMode.None;
            //RenderSettings.FogColor = Color.FloralWhite;
        }

        public void OptimizeFor2D()
        {
            Destroy(_directionalLight);
            Destroy(_demoSceneMenu);
            RenderSettings.Skybox.Enabled = false;
        }

        public void SetControlMode(ControllerSwitcher.ControllerType type, Vector3 position, Vector3 rotation, bool fly = true)
        {
            _controllerSwitcher.DefaultPosition = position;
            _controllerSwitcher.DefaultRotation = rotation;
            _controllerSwitcher.FlyMode = fly;
            _controllerSwitcher.SetControllerActive(type);
        }

        public static void AddLightGroundTest(float range = 50, int lightsCircle = 6)
        {
            var count = (int)(range / 10);

            for (var i = 0; i < count; i++)
                SpawnRadialLights((count + 1) * i, 0, lightsCircle);
        }

        public static void SpawnRadialLights(float radius, float y, int spawnCount, float lightRadius = 5, float intensity = 1)
        {
            Color color;
            Vector3 position;

            for (var i = 0; i < spawnCount; i++)
            {
                var angle = i * MathHelper.TwoPi / 8.0f;

                color = ValidColors[RandomHelper.Range(0, ValidColors.Length)];
                position = new Vector3((float)Math.Cos(angle) * radius, y, (float)Math.Sin(angle) * radius);

                SpawnLight(position, color, lightRadius, intensity, true);
            }
        }

        public static Light SpawnLight(Vector3 position, Color color, float radius, float intensity, bool sinMovement)
        {
            var lightGo = GameObjectFactory.CreateLight(LightType.Point, color, 1.0f, 0);
            lightGo.Transform.LocalRotation = new Vector3(0.0f, 0.5f, 0);
            lightGo.Transform.LocalPosition = position;

            var light = lightGo.GetComponent<Light>();
            light.Radius = radius;
            light.Intensity = intensity;
            light.ShadowEnabled = false;

            var ligthSphere = lightGo.AddComponent<MeshRenderer>();
            ligthSphere.Mesh = new SphereMesh(0.15f, 16);
            ligthSphere.Mesh.Build();
            ligthSphere.CastShadow = true;
            ligthSphere.ReceiveShadow = false;

            ligthSphere.Material = new UnlitMaterial()
            {
                DiffuseColor = color
            };

            if (sinMovement)
                ligthSphere.AddComponent<SinMovement>();

            return light;
        }
    }
}
