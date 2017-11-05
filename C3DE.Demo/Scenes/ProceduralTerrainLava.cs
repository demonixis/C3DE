using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainLava : Scene
    {
        public ProceduralTerrainLava() : base("Procedural Terrain (Lava)") { }

        public override void Initialize()
        {
            base.Initialize();

            // First we add a Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            // And a camera with some components
            var cameraGo = GameObjectFactory.CreateCamera();
            cameraGo.AddComponent<ControllerSwitcher>();
            cameraGo.AddComponent<DemoBehaviour>();
            Add(cameraGo);

            // A light is required to illuminate objects.
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional);
            lightGo.Transform.Rotation = new Vector3(-1, 1, 0);
            Add(lightGo);

            // A terrain with its material.
            var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.Texture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            terrainMaterial.Shininess = 150;
            terrainMaterial.Tiling = new Vector2(8);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Renderer.Geometry.Size = new Vector3(2);
            terrain.Renderer.ReceiveShadow = true;
            terrain.Randomize(4, 12);
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
            Add(terrainGo);

            // Lava
            var lavaTexture = Application.Content.Load<Texture2D>("Textures/lava_texture");
            var lavaNormal = Application.Content.Load<Texture2D>("Textures/lava_bump");
            var lava = GameObjectFactory.CreateLava(lavaTexture, lavaNormal, new Vector3(terrain.Width * 0.5f));
            Add(lava);
        }
    }
}