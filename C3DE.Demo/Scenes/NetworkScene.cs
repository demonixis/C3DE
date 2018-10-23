using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class NetworkScene : SimpleDemo
    {
        public NetworkScene() : base("Network demo") { }

        public NetworkScene(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            m_Camera.AddComponent<NetworkManager>();

            // Terrain
            var terrainMaterial = new StandardMaterial();
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand");
            terrainMaterial.NormalTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Sand_Normal");
            terrainMaterial.Shininess = 500;
            terrainMaterial.Tiling = new Vector2(32);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(4);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Renderer.ReceiveShadow = true;
            terrain.Renderer.CastShadow = false;
            Add(terrainGo);

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.StarsSkybox);

            Screen.ShowCursor = true;
        }
    }
}
