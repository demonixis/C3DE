using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.PostProcess;
using C3DE.Prefabs;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainLava : Scene
    {
        private SimpleBlurPass _blurPostProcess;
        private RefractionPass _refractionPostProcess;
        private BloomPass _bloomPostProcess;

        public ProceduralTerrainLava() : base("Procedural Terrain + Lava") { }

        public override void Initialize()
        {
            base.Initialize();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // Camera
            var camera = new CameraPrefab("camera");
            camera.AddComponent<ControllerSwitcher>();
            camera.AddComponent<DemoBehaviour>();
            Add(camera);

            // Light
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            lightPrefab.Transform.Translate(0, 10, 0);
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            lightPrefab.AddComponent<LightSwitcher>();
            lightPrefab.AddComponent<LightMoverKeys>();
            Add(lightPrefab);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMaterial.Shininess = 50;
            terrainMaterial.Tiling = new Vector2(8);

            var terrain = new TerrainPrefab("terrain");
            terrain.Renderer.Geometry.Size = new Vector3(2);
            terrain.Renderer.ReceiveShadow = true;
            terrain.Randomize(4, 12);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            // Lava
            var lavaMaterial = new LavaMaterial(this);
            lavaMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/lava_texture");
            lavaMaterial.NormalMap = Application.Content.Load<Texture2D>("Textures/lava_bump");

            var lava = new WaterPrefab("water");
            lava.Renderer.Material = lavaMaterial;
            lava.Renderer.ReceiveShadow = true;
            lava.Renderer.Geometry.Size = new Vector3(terrain.Width * 0.5f);
            lava.Renderer.Geometry.Generate();
            Add(lava);

            _blurPostProcess = new SimpleBlurPass();
            //Add(_blurPostProcess);

            _refractionPostProcess = new RefractionPass(Application.Content.Load<Texture2D>("Textures/hexagrid"));
            _refractionPostProcess.TextureTiling = new Vector2(0.5f);
            Add(_refractionPostProcess);

            _bloomPostProcess = new BloomPass();
            _bloomPostProcess.Settings = BloomSettings.PresetSettings[1];
            //Add(_bloomPostProcess);
        }

        public override void Update()
        {
            base.Update();

            _blurPostProcess.BlurDistance = Input.Mouse.Down(Inputs.MouseButton.Left) ? Input.Mouse.Delta.X * 0.001f : 0;
        }
    }
}
