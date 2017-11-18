using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Extensions;
using C3DE.Graphics.Materials;
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
            cameraGo.AddComponent<LightSpawner>();
            Add(cameraGo);

            controller.DefaultPosition = new Vector3(-10.0f, 2.0f, 0.45f);
            controller.DefaultRotation = new Vector3(0.0f, -1.4f, 0.0f);
            controller.SetControllerActive(ControllerSwitcher.ControllerType.FPS);
            controller.FlyMode = true;

            // Sponza Model
            var content = Application.Content;
            var sponzaModel = content.Load<Model>("Models/Atrium/sponza");
            var sponzaGo = sponzaModel.ToMeshRenderers(this);
            var sRenderers = sponzaGo.GetComponentsInChildren<MeshRenderer>();
            foreach (var rd in sRenderers)
            {
                var mat = (StandardMaterial)rd.Material;
                if (mat.MainTexture == null)
                    continue;

                mat.Shininess = 50;
                mat.NormalTexture = content.Load<Texture2D>($"{mat.MainTexture.Name.Replace("_0", "")}_Normal");
            }

            // Sun Flares
            var glowTexture = content.Load<Texture2D>("Textures/Flares/SunGlow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/circle"),
                content.Load<Texture2D>("Textures/Flares/circle_sharp_1"),
                content.Load<Texture2D>("Textures/Flares/circle_soft_1")
            };
            var direction = new Vector3(-1, 1, 0);
            direction.Normalize();

            var sunflares = cameraGo.AddComponent<LensFlare>();
            sunflares.LightDirection = direction;
            sunflares.Setup(glowTexture, flareTextures);

            Screen.ShowCursor = true;

            // Don't miss the Skybox ;)
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // And fog
            //RenderSettings.FogDensity = 0.0085f;
            //RenderSettings.FogMode = FogMode.Exp2;
        }
    }
}
