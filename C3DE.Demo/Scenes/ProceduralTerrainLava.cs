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
    public class ProceduralTerrainLava : Scene
    {
        public ProceduralTerrainLava() : base("Procedural Terrain + Lava") { }

        public override void Initialize()
        {
            base.Initialize();

            // First we add a Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            // And a camera with some components
            var camera = new CameraPrefab("camera");
            camera.AddComponent<ControllerSwitcher>();
            camera.AddComponent<DemoBehaviour>();
            Add(camera);

            // A light is required to illuminate objects.
            var lightPrefab = new LightPrefab("light", LightType.Directional);
            lightPrefab.Transform.Translate(0, 10, 0);
            lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
            lightPrefab.EnableShadows = true;
            lightPrefab.AddComponent<PostProcessSwitcher>();
            Add(lightPrefab);

            // A terrain with its material.
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMaterial.Shininess = 150;
            terrainMaterial.Tiling = new Vector2(8);

            var terrain = new TerrainPrefab("terrain");
            terrain.Renderer.Geometry.Size = new Vector3(2);
            terrain.Renderer.ReceiveShadow = true;
            terrain.Randomize(4, 12);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrain);

            // Lava !
            var lavaMaterial = new LavaMaterial(this);
            lavaMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/lava_texture");
            lavaMaterial.NormalMap = Application.Content.Load<Texture2D>("Textures/lava_bump");

            var lava = new WaterPrefab("water");
            lava.Renderer.Material = lavaMaterial;
            lava.Renderer.ReceiveShadow = true;
            lava.Renderer.Geometry.Size = new Vector3(terrain.Width * 0.5f);
            lava.Renderer.Geometry.Generate();
            Add(lava);
        }
    }
}