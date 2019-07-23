using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scenes;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace C3DE.Demo.Scripts.FPS
{
    using XNAAudioListener = Microsoft.Xna.Framework.Audio.AudioListener;
    using AudioClip = Microsoft.Xna.Framework.Audio.SoundEffect;

    public class PlayerShooter : GameObject
    {
        private Camera _camera;
        private Transform _handTransform;
        private Transform _shootPoint;
        private UnlitMaterial _material;
        private FirstPersonController _fpsController;
        private Vector3 _handLocalPosition;
        private float _fov;
        private float _minFov;
        private float _maxFov;

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            _minFov = 15;
            _maxFov = 65;
            _fov = _maxFov;

            var cameraRig = new GameObject("Head");
            cameraRig.Transform.Parent = Transform;
            cameraRig.Transform.LocalPosition = new Vector3(0, 1.7f, 0);

            var camera = GameObjectFactory.CreateCamera();
            camera.Transform.Parent = cameraRig.Transform;
            camera.Transform.LocalPosition = Vector3.Zero;
            camera.AddComponent<AudioListener>();
            _camera = camera.GetComponent<Camera>();
            _camera.FieldOfView = _fov;
            _camera.Far = 5000;
            _camera.Target = Vector3.Forward;

            var hand = new GameObject("RightHand");
            _handTransform = hand.Transform;
            _handTransform.Parent = cameraRig.Transform;
            _handTransform.LocalPosition = new Vector3(0.25f, -0.4f, -1.75f);
            _handLocalPosition = _handTransform.LocalPosition;

            var model = content.Load<Model>("Models/K9/source/K9_Ravager");
            var gun = model.ToMeshRenderers();
            gun.Transform.LocalScale = new Vector3(0.01f);
            gun.Transform.Parent = hand.Transform;

            var gunMaterial = CreateGunMaterial(content);
            var renderers = gun.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
                renderer.Material = gunMaterial;

            var shootPoint = new GameObject("ShootPoint");
            shootPoint.Transform.Parent = hand.Transform;
            shootPoint.Transform.LocalPosition = Vector3.Forward;
            _shootPoint = shootPoint.Transform;

            _fpsController = AddComponent<FirstPersonController>();
            _fpsController.Fly = false;
            _fpsController.LockCursor = true;

            _material = new UnlitMaterial();
            _material.DiffuseColor = Color.AliceBlue;

            AddComponent<Headbob>();
        }

        public override void Update()
        {
            base.Update();

            if (Input.Mouse.JustClicked(Inputs.MouseButton.Left))
            {
                SpawnProjectile(_shootPoint.Position, _shootPoint.Forward);
                _fpsController.StartCoroutine(StepShoot());
            }

            if (Input.Mouse.JustClicked(Inputs.MouseButton.Right))
            {
                _camera.FieldOfView = _fov < _maxFov ? _maxFov : _minFov;
                _fov = _camera.FieldOfView;
            }

            if (Input.Keys.Escape)
                _fpsController.LockCursor = false;

            if (!_fpsController.LockCursor && Mouse.GetState().LeftButton == ButtonState.Pressed)
                _fpsController.LockCursor = true;
        }

        private void SpawnProjectile(Vector3 position, Vector3 forward)
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

            if (FPSDemo.DebugPhysics)
                rb.AddComponent<RigidbodyRenderer>();

            rb.AddForce(forward * 10);

            var audioSource = go.AddComponent<AudioSource>();
            audioSource.Clip = Application.Content.Load<AudioClip>("Audio/Blaster");
            audioSource.Play();

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

        private Material CreateGunMaterial(ContentManager content)
        {
            if (FPSDemo.PreferePBR)
            {
                var mat = new PBRMaterial()
                {
                    MainTexture = content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Base_Color"),
                    NormalMap = content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Normal_DirectX"),
                    EmissiveMap = content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Emissive"),
                };

                mat.CreateRoughnessMetallicAO(
                    content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Roughness"),
                    content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Metallic"),
                    content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Mixed_AO"));

                return mat;
            }

            return new StandardMaterial()
            {
                MainTexture = content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Base_Color"),
                NormalMap = content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Normal_DirectX"),
                EmissiveMap = content.Load<Texture2D>("Models/K9/textures/MAT_Scifi_Shotty_Emissive"),
                SpecularColor = new Color(new Vector3(0.75f)),
                SpecularPower = 5
            };
        }
    }
}
