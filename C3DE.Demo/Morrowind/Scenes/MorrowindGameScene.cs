using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Demo.Scripts.Lighting;
using C3DE.Demo.Scripts.Utils;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Rendering;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TES3Unity;

namespace C3DE.Morrowind
{
    public class MorrowindGameScene : Scene
    {
        public MorrowindGameScene() : base("MorrowindGame")
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            GUI.Skin = Program.CreateSkin(Application.Content, false);

            // Sun Flares
            var content = Application.Content;
            var glowTexture = content.Load<Texture2D>("Textures/Flares/SunGlow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/circle"),
                content.Load<Texture2D>("Textures/Flares/circle_sharp_1"),
                content.Load<Texture2D>("Textures/Flares/circle_soft_1")
            };

            var sunLight = GameObjectUtils.CreateSunLight(new Vector3(500, 250, 0), new Vector3(50, 330, 0));
            var r = sunLight.AddComponent<LightMoverKeys>();
            var sunflares = sunLight.AddComponent<LensFlare>();
            sunflares.Setup(glowTexture, flareTextures);

            var waterGo = new GameObject("Water");
            waterGo.Transform.Translate(0, -0, 0);
            var terrainW = waterGo.AddComponent<Terrain>();
            terrainW.Geometry.Size = new Vector3(100);
            terrainW.Geometry.TextureRepeat = new Vector2(64);
            terrainW.Flatten();
            terrainW.Geometry.Build();

            var renderer = waterGo.GetComponent<MeshRenderer>();
            renderer.Material = new StandardWaterMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/Fluids/water"),
                NormalMap = content.Load<Texture2D>("Textures/Fluids/Water_Normal"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 4,
                ReflectionIntensity = 0.85f,
                Alpha = 0.9f
            };

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Program.BlueSkybox);

            var engine = new GameObject("Engine");
            engine.AddComponent<TES3Engine>();

            var bloom = new FastBloom(Application.GraphicsDevice);
            bloom.blurIterations = 8;
            bloom.threshold = 0.3f;
            bloom.intensity = 1;
            bloom.blurType = FastBloom.BlurType.Sgx;
            bloom.resolution = FastBloom.Resolution.High;
            AddPostProcess(bloom);

            ForwardRenderer.MaxLightCount = 4;
        }
    }
}
