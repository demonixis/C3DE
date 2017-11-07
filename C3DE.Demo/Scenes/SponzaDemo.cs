using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class SponzaDemo : Scene
    {
        public SponzaDemo() : base("Sponza Demo") { }

        public override void Initialize()
        {
            base.Initialize();

            // And a camera with some components
            var cameraGo = GameObjectFactory.CreateCamera();
            var controller = cameraGo.AddComponent<ControllerSwitcher>();
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);

            controller.DefaultPosition = new Vector3(-10.0f, 2.0f, 0.45f);
            controller.DefaultRotation = new Vector3(0.0f, -1.4f, 0.0f);
            controller.SetControllerActive(ControllerSwitcher.ControllerType.FPS);
            controller.FlyMode = true;

            // Sponza Model
            var content = Application.Content;
            var sponzaGo = GameObjectFactory.CreateXNAModel(content, "Models/Atrium/sponza");
            var modelRenderer = sponzaGo.GetComponent<ModelRenderer>();
            modelRenderer.DrawWithBasicEffect = true;
            Scene.Add(sponzaGo);

            // A Light (which is not yet used because we still use the Basic Effect)
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.LightSkyBlue, 1.0f);
            lightGo.Transform.Rotation = new Vector3(-1, 1, 0);
            Add(lightGo);

            // Sun Flares
            var glowTexture = content.Load<Texture2D>("Textures/Flares/SunGlow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/circle"),
                content.Load<Texture2D>("Textures/Flares/circle_sharp_1"),
                content.Load<Texture2D>("Textures/Flares/circle_soft_1")
            };
            var direction = lightGo.Transform.Rotation;
            direction.Normalize();

            var sunflares = cameraGo.AddComponent<LensFlare>();
            sunflares.LightDirection = direction;
            sunflares.Setup(glowTexture, flareTextures);

            Screen.ShowCursor = true;

            // Don't miss the Skybox ;)
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.Exp2;
        }
    }
}
