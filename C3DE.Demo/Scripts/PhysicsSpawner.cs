using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
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
        private bool m_VREnabled;
        private VRService m_VRService;
        private Transform m_RightHand;
        private Transform m_LeftHand;
        private StandardMaterial m_Material;

        public override void Start()
        {
            VRManager.VRServiceChanged += OnVRChanged;

            m_Material = new StandardMaterial();
            m_Material.DiffuseColor = RandomHelper.GetColor();
            m_Material.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");
            m_Material.NormalTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock_Normal");

            var go = new GameObject("Cube");
            var cube = go.AddComponent<MeshRenderer>();
            cube.Geometry = new CubeMesh();
            cube.Geometry.Build();
            cube.CastShadow = true;
            cube.ReceiveShadow = false;
            cube.Material = m_Material;
        }

        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.Space) || Input.Keys.Pressed(Keys.LeftControl))
                SpawnCubeAtPosition(Camera.Main.Transform.Position, Camera.Main.Transform.Forward);

            if (m_VRService != null && m_RightHand != null && m_VRService.GetButtonDown(1, XRButton.Trigger))
                SpawnCubeAtPosition(m_RightHand.Position, m_RightHand.Forward);

            if (m_VRService != null && m_LeftHand != null && m_VRService.GetButtonDown(0, XRButton.Trigger))
                SpawnCubeAtPosition(m_LeftHand.Position, m_LeftHand.Forward);
        }

        private void SpawnCubeAtPosition(Vector3 position, Vector3 forward)
        {
            var go = new GameObject("Cube");
            go.Transform.LocalPosition = position;

            var cube = go.AddComponent<MeshRenderer>();
            cube.Geometry = new CubeMesh();
            cube.Geometry.Size = new Vector3(m_VREnabled ? 0.25f : 1.0f);
            cube.Geometry.Build();
            cube.CastShadow = true;
            cube.ReceiveShadow = true;
            cube.Material = m_Material;

            var collider = cube.AddComponent<BoxCollider>();
            var rb = cube.AddComponent<Rigidbody>();
            rb.AddComponent<RigidbodyRenderer>();
            rb.AddForce(forward * (m_VREnabled ? 5 : 800));
        }

        private void OnVRChanged(VRService service)
        {
            m_VRService = service;
            m_VREnabled = service != null;
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
                    m_RightHand = controller.Transform;
                else
                    m_LeftHand = controller.Transform;
            }
        }
    }
}
