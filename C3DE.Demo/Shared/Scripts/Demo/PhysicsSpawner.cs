using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Components.VR;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Utils;
using C3DE.VR;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace C3DE.Demo.Scripts
{
    public class PhysicsSpawner : Behaviour
    {
        private bool _VREnabled;
        private VRService _VRService;
        private Transform _rightHand;
        private Transform _leftHand;
        private StandardMaterial _material;

        public override void Start()
        {
            VRManager.VRServiceChanged += OnVRChanged;

            _material = new StandardMaterial();
            _material.DiffuseColor = RandomHelper.GetColor();
            _material.MainTexture = Application.Content.Load<Texture2D>("Textures/Proto/Protogrid");
            _material.EmissiveMap = TextureFactory.MergeTextures(BlendState.NonPremultiplied, _material.MainTexture, RandomHelper.GetColor());
            _material.EmissiveColor = RandomHelper.GetColor();
            _material.Tiling = new Vector2(0.5f);

            var go = new GameObject("Cube");
            var cube = go.AddComponent<MeshRenderer>();
            cube.Mesh = new CubeMesh();
            cube.Mesh.Build();
            cube.CastShadow = true;
            cube.ReceiveShadow = false;
            cube.Material = _material;
        }

        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.Space) || Input.Keys.Pressed(Keys.LeftControl))
                SpawnCubeAtPosition(Camera.Main.Transform.Position, Camera.Main.Transform.Forward);

            if (_VRService != null && _rightHand != null && _VRService.GetButtonDown(1, XRButton.Trigger))
                SpawnCubeAtPosition(_rightHand.Position, _rightHand.Forward);

            if (_VRService != null && _leftHand != null && _VRService.GetButtonDown(0, XRButton.Trigger))
                SpawnCubeAtPosition(_leftHand.Position, _leftHand.Forward);
        }

        private void SpawnCubeAtPosition(Vector3 position, Vector3 forward)
        {
            var go = new GameObject("Cube");
            go.Transform.LocalPosition = position;

            var cube = go.AddComponent<MeshRenderer>();
            cube.Mesh = new CubeMesh();
            cube.Mesh.Size = new Vector3(_VREnabled ? 0.25f : 1.0f);
            cube.Mesh.Build();
            cube.CastShadow = true;
            cube.ReceiveShadow = true;
            cube.Material = _material;

            var collider = cube.AddComponent<BoxCollider>();
            var rb = cube.AddComponent<Rigidbody>();
            rb.AddComponent<RigidbodyRenderer>();
            rb.AddForce(forward * (_VREnabled ? 5 : 800));
        }

        private void OnVRChanged(VRService service)
        {
            _VRService = service;
            _VREnabled = service != null;
            StartCoroutine(SetupVRPlayer());
        }

        private IEnumerator SetupVRPlayer()
        {
            yield return Coroutine.WaitForSeconds(0.5f);

            var player = Camera.Main.Transform.Parent;
            if (player == null)
                yield break;

            var controllers = player.GetComponentsInChildren<MotionController>();
            foreach (var controller in controllers)
            {
                if (!controller.LeftHand)
                    _rightHand = controller.Transform;
                else
                    _leftHand = controller.Transform;
            }
        }
    }
}
