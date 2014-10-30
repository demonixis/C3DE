using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainWater : Scene
    {
        public ProceduralTerrainWater() : base("Procedural Terrain + Water") { }  

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
            lightPrefab.Light.Range = 25;
            lightPrefab.Light.Intensity = 1.0f;
            lightPrefab.Light.FallOf = 5f;
            
            lightPrefab.Light.Direction = new Vector3(0, 0.5f, 1);
            lightPrefab.Light.Angle = MathHelper.PiOver4;
            lightPrefab.Light.ShadowGenerator.ShadowStrength = 1f;
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            lightPrefab.AddComponent<LightSwitcher>();
            Add(lightPrefab);

            // Terrain
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMaterial.Shininess = 50;
            terrainMaterial.Tiling = new Vector2(8);

            var terrain = new TerrainPrefab("terrain");
            terrain.Randomize(4, 12);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            var water = new WaterPrefab("water");
            Add(water);
            water.Generate("Textures/water", "Textures/wavesbump", new Vector3(terrain.Width * 0.5f));
            (water.Renderer.Material as WaterMaterial).ReflectiveMap = scene.RenderSettings.Skybox.Texture;
            (water.Renderer.Material as WaterMaterial).WaterTransparency = 0.6f;
        }
    }
}
