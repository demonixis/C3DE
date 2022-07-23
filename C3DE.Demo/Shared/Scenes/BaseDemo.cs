using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Demo.Scripts.Diagnostic;
using C3DE.Demo.Scripts.Utils;
using C3DE.Demo.Scripts.VR;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scenes
{
    public class BaseDemo : Scene
    {
        public static bool PreferePBRMaterials = false;

        protected static Color[] ValidColors = new[]
        {
            Color.Red, Color.Green, Color.Blue,
            Color.Purple, Color.Cyan, Color.Yellow
        };

        protected Camera _camera;
        protected Light _directionalLight;
        protected ControllerSwitcher _controllerSwitcher;
        protected DemoSceneMenu _demoSceneMenu;
        protected VRPlayerEnabler _vrPlayerEnabler;

        public BaseDemo(string name) : base(name) { }

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
            _camera.AddComponent<StatsDisplay>();

            // Scripts
            _controllerSwitcher = _camera.AddComponent<ControllerSwitcher>();
            _demoSceneMenu = _camera.AddComponent<DemoSceneMenu>();
            _vrPlayerEnabler = _camera.AddComponent<VRPlayerEnabler>();

            // Main light
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

           /* var sunflares = _directionalLight.AddComponent<LensFlare>();
            sunflares.Setup(glowTexture, flareTextures);*/
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
            Destroy(_vrPlayerEnabler);
            RenderSettings.Skybox.Enabled = false;
        }

        public void SetControlMode(ControllerSwitcher.ControllerType type, Vector3 position, Vector3 rotation, bool fly = true)
        {
            _controllerSwitcher.DefaultPosition = position;
            _controllerSwitcher.DefaultRotation = rotation;
            _controllerSwitcher.FlyMode = fly;
            _controllerSwitcher.SetControllerActive(type);
        }

        #region Materials Creation

        public Material GetLavaMaterial(ContentManager content)
        {
            if (PreferePBRMaterials)
            {
                return new PBRLavaMaterial
                {
                    MainTexture = content.Load<Texture2D>("Textures/Fluids/lava_texture"),
                    NormalMap = content.Load<Texture2D>("Textures/Fluids/wavesbump"),
                    Metallic = 0,
                    Roughness = 0
                };
            }

            return new StandardLavaMaterial
            {
                MainTexture = content.Load<Texture2D>("Textures/Fluids/lava_texture"),
                NormalMap = content.Load<Texture2D>("Textures/Fluids/wavesbump"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 50
            };
        }

        public Material GetWaterMaterial(ContentManager content, TextureCube reflectionMap)
        {
            var alpha = 0.6f;

            if (PreferePBRMaterials)
            {
                var waterMaterial = new PBRWaterMaterial
                {
                    MainTexture = content.Load<Texture2D>("Textures/Fluids/water"),
                    NormalMap = content.Load<Texture2D>("Textures/Fluids/wavesbump"),
                    Alpha = alpha
                };

                waterMaterial.CreateRoughnessMetallicAO(0.0f, 0.0f, 1.0f);

                return waterMaterial;
            }

            return new StandardWaterMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/Fluids/water"),
                NormalMap = content.Load<Texture2D>("Textures/Fluids/Water_Normal"),
                SpecularColor = new Color(0.7f, 0.7f, 0.7f),
                SpecularPower = 4,
                ReflectionMap = reflectionMap,
                ReflectionIntensity = 0.85f,
                Alpha = alpha
            };
        }

        public Material GetTerrainMaterial(ContentManager content, Texture2D weightMap, float tiling = 8)
        {
            if (PreferePBRMaterials)
            {
                var terrainMaterial = new PBRTerrainMaterial
                {
                    GrassMap = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_col"),
                    GrassNormalMap = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_nrm"),
                    SandMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_col"),
                    SandNormalMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_nrm"),
                    SnowMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_col"),
                    SnowNormalMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_nrm"),
                    RockMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_col"),
                    RockNormalMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_nrm"),
                    WeightMap = weightMap,
                    Metallic = 0.2f,
                    Roughness = 0.5f,
                    Tiling = new Vector2(tiling)
                };
            }

            return new StandardTerrainMaterial
            {
                MainTexture = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_col"),
                GrassNormalMap = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_nrm"),
                SandMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_col"),
                SandNormalMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_nrm"),
                SnowMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_col"),
                SnowNormalMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_nrm"),
                RockMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_col"),
                RockNormalMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_nrm"),
                WeightMap = weightMap,
                SpecularColor = Color.Gray,
                SpecularIntensity = 1,
                SpecularPower = 32,
                Tiling = new Vector2(tiling)
            };
        }

        #endregion

        #region Add Test Lights

        public void AddLightGroundTest(float range = 50, int lightsCircle = 6, bool showLights = true)
        {
            var count = (int)(range / 10);
            var go = new GameObject("LightGroupTest");
            GameObject target;

            for (var i = 0; i < count; i++)
            {
                target = SpawnRadialLights((count + 1) * i, 0, lightsCircle, 5, 1);
                target.Transform.Parent = go.Transform;
            }
        }

        public GameObject SpawnRadialLights(float radius, float y, int spawnCount, float lightRadius = 5, float intensity = 1)
        {
            Vector3 position;
            Light light;
            Material material;

            var go = new GameObject($"RadialLight_{radius}");

            var mesh = new SphereMesh(0.1f, 16);
            mesh.Build();

            var matCount = ValidColors.Length;
            var materials = new UnlitMaterial[matCount];
            for (var i = 0; i < matCount; i++)
                materials[i] = new UnlitMaterial { DiffuseColor = ValidColors[i] };

            for (var i = 0; i < spawnCount; i++)
            {
                material = materials[RandomHelper.Range(0, materials.Length)];

                var angle = i * MathHelper.TwoPi / 8.0f;
                position = new Vector3((float)Math.Cos(angle) * radius, y, (float)Math.Sin(angle) * radius);

                light = SpawnLight(position, material.DiffuseColor, lightRadius, intensity, true, mesh, material);
                light.Transform.Parent = go.Transform;
            }

            return go;
        }

        public static Light SpawnLight(Vector3 position, Color color, float radius = 5.0f, float intensity = 1.0f, bool sinMovement = false, Mesh mesh = null, Material material = null)
        {
            var lightGo = GameObjectFactory.CreateLight(LightType.Point, color, 1.0f, 0);
            lightGo.Transform.LocalRotation = new Vector3(0.0f, 0.5f, 0);
            lightGo.Transform.LocalPosition = position;

            var light = lightGo.GetComponent<Light>();
            light.Radius = radius;
            light.Intensity = intensity;
            light.ShadowEnabled = false;

            if (mesh != null && material != null)
            {
                var ligthSphere = lightGo.AddComponent<MeshRenderer>();
                ligthSphere.Mesh = mesh;
                ligthSphere.CastShadow = true;
                ligthSphere.ReceiveShadow = false;
                ligthSphere.Material = material;
            }

            if (sinMovement)
                lightGo.AddComponent<SinMovement>();

            return light;
        }

        #endregion
    }
}
