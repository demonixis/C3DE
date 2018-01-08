using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Primitives;
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
            Add(head);
            head.Transform.LocalPosition = new Vector3(0, 1.8f, 0);
            trackingSpace.Transform.Parent = head.Transform;

            var player = new GameObject();
            Add(player);
            head.Transform.Parent = player.Transform;
            player.AddComponent<VRSwitcher>();

            CreateHandController(true);
            CreateHandController(false);

            BuildScene();
        }

        private void CreateHandController(bool leftHand)
        {
            var handString = leftHand ? "Left" : "Right";
            var hand = new GameObject($"{handString}HandAnchor");
            Add(hand);

            var motionController = hand.AddComponent<MotionController>();
            motionController.LeftHand = leftHand; ;

            var modelRaw = Application.Content.Load<Model>($"Models/VR/{handString}HandModel");
            var model = modelRaw.ToMeshRenderers(this);
            model.Transform.Parent = hand.Transform;

            var handMaterial = new StandardMaterial(m_Scene);
            handMaterial.DiffuseColor = Color.DarkBlue;

            var sphereHand = hand.AddComponent<MeshRenderer>();
            sphereHand.Geometry = new CubeMesh();
            sphereHand.Geometry.Size = new Vector3(0.05f);
            sphereHand.Geometry.Build();
            sphereHand.CastShadow = true;
            sphereHand.ReceiveShadow = true;
            sphereHand.Material = handMaterial;
        }

        private void BuildScene()
        {
            var lightGo = GameObjectFactory.CreateLight(LightType.Point, Color.LightSteelBlue, 1.5f);
            lightGo.Transform.LocalPosition = new Vector3(-20, 20, 0);
            lightGo.Transform.LocalRotation = new Vector3(1, -1, 0);
            lightGo.AddComponent<DemoBehaviour>();
            Add(lightGo);

            lightGo.GetComponent<Light>().Range = 100;

            // Terrain
            var terrainMaterial = new StandardMaterial(m_Scene);
            terrainMaterial.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Grass");
            terrainMaterial.Shininess = 150;
            terrainMaterial.Tiling = new Vector2(16);

            var terrainGo = GameObjectFactory.CreateTerrain();
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.Renderer.Geometry.Size = new Vector3(2);
            terrain.Renderer.Geometry.TextureRepeat = new Vector2(4);
            terrain.Renderer.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.CastShadow = true;
            terrain.Renderer.Material = terrainMaterial;
            Add(terrainGo);

            var cubMat = new StandardMaterial(this);
            cubMat.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.Red, Color.White);
            cubMat.Tiling = new Vector2(2, 2);

            for (var i = 0; i < 10; i++)
            {
                var go = new GameObject("Cube " + i);
                go.Transform.LocalPosition = RandomHelper.GetVector3(-20, 1, -20, 20, 1, 20);
                Add(go);
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.Geometry = new CubeMesh();
                renderer.Geometry.Build();
                renderer.Material = cubMat;
            }

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);
        }
    }
}