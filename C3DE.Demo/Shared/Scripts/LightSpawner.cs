using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using C3DE.VR;
using System.Collections;

namespace C3DE.Demo.Scripts
{
    public sealed class LightSpawner : Behaviour
    {
        private VRService _VRService;
        private Transform _rightHand;

        public bool ShowDebugMesh { get; set; } = false;
        public bool ShadowMapEnabled { get; set; } = false;
        public float Intensity { get; set; } = 1.0f;
        public Color? Color { get; set; } = null;
  
        public override void Start()
        {
            VRManager.VRServiceChanged += OnVRChanged;
        }

        private void OnVRChanged(VRService service)
        {
            _VRService = service;

            StartCoroutine(SetupVRPlayer());
        }

        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.Space))
                SpawnLightAt(_transform.Position);

            if (_VRService != null && _rightHand != null && _VRService.GetButtonDown(1, XRButton.Trigger))
                SpawnLightAt(_rightHand.Position);
        }

        private void SpawnLightAt(Vector3 position)
        {
            var color = Color.HasValue ? Color.Value : RandomHelper.GetColor();
            var lightGo = GameObjectFactory.CreateLight(LightType.Point, color, Intensity, 1024);
            lightGo.Transform.LocalPosition = position;
            Scene.current.Add(lightGo);

            var light = lightGo.GetComponent<Light>();
            light.Radius = 5;
            light.ShadowGenerator.ShadowStrength = 1;
            light.ShadowGenerator.Enabled = ShadowMapEnabled;

            if (ShowDebugMesh)
            {
                var ligthSphere = lightGo.AddComponent<MeshRenderer>();
                ligthSphere.Mesh = new SphereMesh(0.05f, 12);
                ligthSphere.Mesh.Build();
                ligthSphere.CastShadow = true;
                ligthSphere.ReceiveShadow = false;

                var sphereMaterial = new UnlitMaterial();
                sphereMaterial.DiffuseColor = color;
                ligthSphere.Material = sphereMaterial;
            }
        }

        private IEnumerator SetupVRPlayer()
        {
            yield return Coroutine.WaitForSeconds(0.5f);

            var player = Camera.Main.Transform.Parent;
            if (player != null)
            {
                var controllers = player.GetComponentsInChildren<MotionController>();
                foreach (var controller in controllers)
                    if (!controller.LeftHand)
                        _rightHand = controller.Transform;
            }
        }
    }
}
