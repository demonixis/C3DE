using System;
using C3DE.Components;
using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Demo.Scripts;
using C3DE.Geometries;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Rendering;
using C3DE.Utils;
using C3DE.VR;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scenes
{
	public class VirtualRealityDemo : Scene
    {
        private Rendering.Renderer _prevRenderer;

        public VirtualRealityDemo() : base("Virtual Reality demo") { }

        public override void Initialize()
        {
            base.Initialize();

			var cameraNode = new GameObject();
			Add(cameraNode);
			cameraNode.AddComponent<Camera>();

			var trackingSpace = new GameObject();
			Add(trackingSpace);
			cameraNode.Transform.Parent = trackingSpace.Transform;

			var head = new GameObject();
			head.Transform.Position = new Vector3(0, 1.8f, 0);
			Add(head);
			trackingSpace.Transform.Parent = head.Transform;

			var player = new GameObject();
			Add(player);
			head.Transform.Parent = player.Transform;

            _prevRenderer = Application.Engine.Renderer;

			var vrDevice = new OSVRService(Application.Engine);
			if (vrDevice.TryInitialize() == 0)
			{
				var vrRenderer = new VRRenderer(Application.GraphicsDevice, vrDevice);
				Application.Engine.Renderer = vrRenderer;
			}

            BuildScene();
		}

        public override void Unload()
        {
            Application.Engine.Renderer = _prevRenderer;
            base.Unload();
        }

        private void BuildScene()
        {
			var lightPrefab = new LightPrefab("lightPrefab", LightType.Directional);
			Add(lightPrefab);
			lightPrefab.Transform.Position = new Vector3(-15, 15, 15);
			lightPrefab.Transform.Rotation = new Vector3(0, MathHelper.Pi, 1);
			lightPrefab.Light.Range = 105;
			lightPrefab.Light.Intensity = 2.0f;
			lightPrefab.Light.FallOf = 5f;
			lightPrefab.Light.Color = Color.Violet;
			lightPrefab.Transform.Rotation = new Vector3(-1, 1, 0);
			lightPrefab.Light.ShadowGenerator.ShadowStrength = 0.6f; // FIXME need to be inverted
			lightPrefab.Light.ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, 1024);
			lightPrefab.EnableShadows = true;
			lightPrefab.AddComponent<DemoBehaviour>();

			// Terrain
			var terrainMaterial = new StandardMaterial(scene);
			terrainMaterial.Texture = GraphicsHelper.CreateBorderTexture(Color.LightGreen, Color.LightSeaGreen, 128, 128, 4);
			terrainMaterial.Shininess = 10;
			terrainMaterial.Tiling = new Vector2(64);

			var terrain = new TerrainPrefab("terrain");
			terrain.Renderer.Geometry.Size = new Vector3(2);
			terrain.Renderer.Geometry.Build();
			terrain.Flatten();
			terrain.Renderer.Material = terrainMaterial;
			terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);
			Add(terrain);

			var cubMat = new StandardMaterial(this);
			cubMat.Texture = GraphicsHelper.CreateCheckboardTexture(Color.Red, Color.White);
			cubMat.Tiling = new Vector2(2, 2);

			for (var i = 0; i < 10; i++)
			{
				var go = new GameObject("Cube " + i);
				go.Transform.Position = RandomHelper.GetVector3(-20, 0.5f, -20, 20, 0.5f, 20);
				Add(go);
				var renderer = go.AddComponent<MeshRenderer>();
				renderer.Geometry = new CubeGeometry();
				renderer.Geometry.Build();
				renderer.Material = cubMat;
			}

			// Skybox
			RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox, 100);
        }
    }
}