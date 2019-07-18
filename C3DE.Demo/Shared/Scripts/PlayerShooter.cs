using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class PlayerShooter : GameObject
    {
        private Transform _shootPoint;
        private UnlitMaterial _material;

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            var cameraRig = new GameObject("Head");
            cameraRig.Transform.Parent = Transform;
            cameraRig.Transform.LocalPosition = new Vector3(0, 1.7f, 0);

            var camera = GameObjectFactory.CreateCamera();
            camera.Transform.Parent = cameraRig.Transform;
            camera.Transform.LocalPosition = Vector3.Zero;

            var hand = new GameObject("RightHand");
            hand.Transform.Parent = cameraRig.Transform;
            hand.Transform.LocalPosition = new Vector3(0.25f, -0.4f, -1.0f);

            var model = content.Load<Model>("Models/Pistol/Model/Pistol");
            var gun = model.ToMeshRenderers();
            gun.Transform.LocalScale = new Vector3(0.01f);
            gun.Transform.Parent = hand.Transform;
            gun.Transform.Rotate(0, MathHelper.PiOver2, 0);

            var shootPoint = new GameObject("ShootPoint");
            shootPoint.Transform.Parent = hand.Transform;
            shootPoint.Transform.LocalPosition = Vector3.Forward;
            _shootPoint = shootPoint.Transform;

            var renderers = gun.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                var std = (StandardMaterial)renderer.Material;
                std.MainTexture = content.Load<Texture2D>("Models/Pistol/pistol_skin");
                std.SpecularTexture = content.Load<Texture2D>("Models/Pistol/pistol_spec");
                std.EmissiveMap = content.Load<Texture2D>("Models/Pistol/pistol_emissive");
            }

            var fpsController = AddComponent<FirstPersonController>();
            fpsController.Fly = false;
            fpsController.LockCursor = true;

            _material = new UnlitMaterial();
            _material.DiffuseColor = Color.Red;
        }

        public override void Update()
        {
            base.Update();

            if (Input.Mouse.JustClicked(Inputs.MouseButton.Left))
                SpawnCubeAtPosition(_shootPoint.Position, _shootPoint.Forward);
        }

        private void SpawnCubeAtPosition(Vector3 position, Vector3 forward)
        {
            var go = new GameObject("Cube");
            go.Transform.LocalPosition = position;

            var cube = go.AddComponent<MeshRenderer>();
            cube.Mesh = new SphereMesh();
            cube.Mesh.Size = new Vector3(0.1f);
            cube.Mesh.Build();
            cube.CastShadow = true;
            cube.ReceiveShadow = true;
            cube.Material = _material;
            cube.AddComponent<SphereCollider>();

            var rb = cube.AddComponent<Rigidbody>();
            rb.AddComponent<RigidbodyRenderer>();
            rb.AddForce(forward * 10);
        }
    }
}
