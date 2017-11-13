using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Geometries;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering;
using C3DE.Utils;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
	public class VirtualRealityDemo : Scene
    {
        public VirtualRealityDemo() : base("Virtual Reality") { }

        public override void Initialize()
        {
            base.Initialize();

            var cameraGo = GameObjectFactory.CreateCamera();
			Add(cameraGo);

			var trackingSpace = new GameObject();
			Add(trackingSpace);
			cameraGo.Transform.Parent = trackingSpace.Transform;

			var head = new GameObject();
			head.Transform.Position = new Vector3(0, 1.8f, 0);
			Add(head);
			trackingSpace.Transform.Parent = head.Transform;

			var player = new GameObject();
			Add(player);
			head.Transform.Parent = player.Transform;

            var vrDevice = GetService();
            if (vrDevice.TryInitialize() == 0)
                Application.Engine.Renderer.SetVREnabled(vrDevice);

            BuildScene();
		}

        private VRService GetService()
        {
#if DESKTOPGL
            return new OSVRService(Application.Engine);
#else
            return new OpenVRService(Application.Engine);
#endif
        }

        public override void Unload()
        {
            Application.Engine.Renderer.SetVREnabled(null);
        }

        private void BuildScene()
        {
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.LightSteelBlue, 0.5f);
            lightGo.Transform.Position = new Vector3(-15, 15, 15);
            lightGo.Transform.Rotation = new Vector3(-1, -1, 0);
            lightGo.AddComponent<DemoBehaviour>();
            lightGo.AddComponent<LightMover>();
            Add(lightGo);

			// Terrain
			var terrainMaterial = new StandardMaterial(scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
			terrainMaterial.Shininess = 150;
			terrainMaterial.Tiling = new Vector2(16);
            terrainMaterial.EmissiveTexture = terrainMaterial.MainTexture;
            terrainMaterial.EmissiveEnabled = true;

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Renderer.Geometry.Size = new Vector3(2);
            terrain.Renderer.Geometry.TextureRepeat = new Vector2(4);
            terrain.Renderer.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
			Add(terrainGo);

			var cubMat = new StandardMaterial(this);
			cubMat.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.Red, Color.White);
			cubMat.Tiling = new Vector2(2, 2);

			for (var i = 0; i < 10; i++)
			{
				var go = new GameObject("Cube " + i);
				go.Transform.Position = RandomHelper.GetVector3(-20, 1, -20, 20, 1, 20);
				Add(go);
				var renderer = go.AddComponent<MeshRenderer>();
				renderer.Geometry = new CubeGeometry();
				renderer.Geometry.Build();
				renderer.Material = cubMat;
			}

			// Skybox
			RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);
        }
    }
}