using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Demo.Scripts;

namespace C3DE
{
    public enum GeometryType
    {
        Cube = 0, Sphere, Cylinder,
    }

    public static class GameObjectFactory
    {
        public static GameObject CreateCamera() => CreateCamera(new Vector3(0, 0, -10), new Vector3(0, 0, 0), Vector3.Up);

        public static GameObject CreateCamera(Vector3 position, Vector3 target, Vector3 upVector)
        {
            var gameObject = new GameObject("Camera");
            var camera = gameObject.AddComponent<Camera>();
            camera.Setup(position, target, upVector);
            return gameObject;
        }

        public static GameObject CreateLight(LightType type) => CreateLight(type, Color.White);

        public static GameObject CreateLight(LightType type, Color color, float intensity = 1.0f, int shadowMapSize = 1024)
        {
            var gameObject = new GameObject($"Light_{type}");
            var light = gameObject.AddComponent<Light>();
            light.TypeLight = type;
            light.Intensity = intensity;
            light._color = color.ToVector3();

            if (shadowMapSize > 0)
                light.m_ShadowGenerator.SetShadowMapSize(Application.GraphicsDevice, shadowMapSize);

            return gameObject;
        }

        public static GameObject CreateMesh(Mesh geometry, bool receiveShadow = true, bool castShadow = true, bool collider = true)
        {
            var gameObject = new GameObject($"Mesh_{nameof(geometry)}");
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.Mesh = geometry;
            renderer.ReceiveShadow = receiveShadow;
            renderer.CastShadow = castShadow;

            if (collider)
                gameObject.AddComponent<BoxCollider>();

            if (geometry != null && !geometry.Built)
                geometry.Build();

            return gameObject;
        }

        public static GameObject CreateMesh(GeometryType type, bool receiveShadow = true, bool castShadow = true, bool collider = true)
        {
            Mesh mesh = null;
            if (type == GeometryType.Cube)
                mesh = new CubeMesh();
            else if (type == GeometryType.Sphere)
                mesh = new SphereMesh(1, 64);
            else
                mesh = new CylinderMesh();

            var gameObject = new GameObject($"Mesh_{type}");
            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.Mesh = mesh;
            renderer.ReceiveShadow = receiveShadow;
            renderer.CastShadow = castShadow;

            if (collider)
                gameObject.AddComponent<BoxCollider>();

            if (mesh != null && !mesh.Built)
                mesh.Build();

            return gameObject;
        }

        public static GameObject CreateXNAModel(Model model)
        {
            var gameObject = new GameObject("Model");
            var renderer = gameObject.AddComponent<ModelRenderer>();
            var collider = gameObject.AddComponent<SphereCollider>();
            var sphere = new BoundingSphere();

            renderer.Model = model;

            foreach (var mesh in renderer.Model.Meshes)
                sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);

            collider.Sphere = sphere;

            return gameObject;
        }

        public static GameObject CreateXNAModel(ContentManager content, string modelPath) => CreateXNAModel(content.Load<Model>(modelPath));

        public static GameObject CreateTerrain()
        {
            var gameObject = new GameObject("Terrain");
            var renderer = gameObject.AddComponent<Terrain>();
            return gameObject;
        }

        public static GameObject CreateLava(Texture2D lavalTexture, Texture2D normalTexture, Vector3 size)
        {
            var gameObject = new GameObject("Lava");

            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.CastShadow = false;
            renderer.ReceiveShadow = false;
            renderer.Mesh = new PlaneMesh();

            var collider = gameObject.AddComponent<BoxCollider>();
            collider.IsPickable = false;

            var material = new LavaMaterial();
            material.MainTexture = lavalTexture;
            material.NormalTexture = normalTexture;

            renderer.Material = material;
            renderer.Mesh.Size = size;
            renderer.Mesh.Build();

            //collider.BoundingBox = new BoundingBox(transform.Position, size);

            return gameObject;
        }

        public static GameObject CreateWater(Texture2D waterTexture, Texture2D normalTexture, Vector3 size)
        {
            var gameObject = new GameObject("Water");

            var renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.CastShadow = false;
            renderer.ReceiveShadow = false;
            renderer.Mesh = new PlaneMesh();

            var collider = gameObject.AddComponent<BoxCollider>();
            collider.IsPickable = false;

            var material = new StandardWaterMaterial();
            material.MainTexture = waterTexture;
            material.NormalMap = normalTexture;

            renderer.Material = material;
            renderer.Mesh.Size = size;
            renderer.Mesh.Build();

            //collider.BoundingBox = new BoundingBox(transform.Position, size);

            return gameObject;
        }

        public static GameObject CreatePlayer(bool vrEnabled)
        {
            var scene = Scene.current;

            var player = new GameObject("Player");
            scene.Add(player);

            var head = new GameObject();
            head.Transform.Parent = player.Transform;
            head.Transform.LocalPosition = new Vector3(0, 1.8f, 0);
            scene.Add(head);

            var trackingSpace = new GameObject();
            trackingSpace.Transform.Parent = head.Transform;
            scene.Add(trackingSpace);

            var cameraGo = CreateCamera();
            cameraGo.Name = "CenterEyeAnchor";
            cameraGo.Transform.Parent = trackingSpace.Transform;
            scene.Add(cameraGo);

            if (vrEnabled)
            {
                var leftHand = new GameObject("LeftHandAnchor");
                leftHand.Transform.Parent = trackingSpace.Transform;
                scene.Add(leftHand);

                var mc = leftHand.AddComponent<MotionController>();
                mc.LeftHand = true;

                var rightHand = new GameObject("RightHandAnchor");
                rightHand.Transform.Parent = trackingSpace.Transform;
                scene.Add(rightHand);

                mc = rightHand.AddComponent<MotionController>();
                mc.LeftHand = false;
            }

            return player;
        }
    }
}
