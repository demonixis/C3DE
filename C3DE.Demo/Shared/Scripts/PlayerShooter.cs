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
using System.Collections;

namespace C3DE.Demo.Scripts
{
    public class PlayerShooter : GameObject
    {
        private Transform _handTransform;
        private Transform _shootPoint;
        private UnlitMaterial _material;
        private FirstPersonController _fpsController;
        private Vector3 _handLocalPosition;

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
            _handTransform = hand.Transform;
            _handTransform.Parent = cameraRig.Transform;
            _handTransform.LocalPosition = new Vector3(0.25f, -0.4f, -1.25f);
            _handLocalPosition = _handTransform.LocalPosition;

            var model = content.Load<Model>("Models/ScifiGun/source/ScifiGun");
            var gun = model.ToMeshRenderers();
            gun.Transform.LocalScale = new Vector3(0.001f);
            gun.Transform.Parent = hand.Transform;
            gun.Transform.Rotate(0, MathHelper.PiOver2, 0);

            var renderers = gun.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                var std = (StandardMaterial)renderer.Material;
                std.MainTexture = content.Load<Texture2D>("Models/ScifiGun/textures/ChumboScyFi_DIFF");
                std.SpecularTexture = content.Load<Texture2D>("Models/ScifiGun/textures/ChumboScyFi_SPEC");
                std.NormalMap = content.Load<Texture2D>("Models/ScifiGun/textures/ChumboScyFi_NRM");
                std.EmissiveMap = content.Load<Texture2D>("Models/ScifiGun/textures/ChumboScyFi_Illum");
                std.SpecularPower = 25;
            }

            var shootPoint = new GameObject("ShootPoint");
            shootPoint.Transform.Parent = hand.Transform;
            shootPoint.Transform.LocalPosition = Vector3.Forward;
            _shootPoint = shootPoint.Transform;

            _fpsController = AddComponent<FirstPersonController>();
            _fpsController.Fly = false;
           // _fpsController.LockCursor = true;

            _material = new UnlitMaterial();
            _material.DiffuseColor = Color.Red;
        }

        public override void Update()
        {
            base.Update();

            if (Input.Mouse.JustClicked(Inputs.MouseButton.Left))
            {
                SpawnCubeAtPosition(_shootPoint.Position, _shootPoint.Forward);
                _fpsController.StartCoroutine(StepShoot());
            }
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

            _fpsController.StartCoroutine(DestroyAfter(go));
        }

        private IEnumerator DestroyAfter(GameObject go)
        {
            yield return Coroutine.WaitForSeconds(2.5f);
            Destroy(go);
        }

        private IEnumerator StepShoot()
        {
            _handTransform.LocalPosition = _handLocalPosition;

            var target = _handLocalPosition + new Vector3(0, 0, 0.25f);

            while (Vector3.Distance(_handTransform.LocalPosition, target) > 0.1f)
            {
                _handTransform.LocalPosition = Vector3.Lerp(_handTransform.LocalPosition, target, Time.DeltaTime * 5f);
                yield return null;
            }

            _handTransform.LocalPosition = _handLocalPosition;
        }
    }
}
