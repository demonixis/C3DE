using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Demo.Scripts
{
    public class VRSwitcher : Behaviour
    {
        private Rectangle _UIRectangle;
        private bool _VREnabled = false;
        private GameObject[] _hands;

        public bool Automatic { get; set; } = true;

        public Point UIPosition
        {
            get => new Point(_UIRectangle.X, _UIRectangle.Y);
            set
            {
                _UIRectangle.X = value.X;
                _UIRectangle.Y = value.Y;
            }
        }

        public Action<bool> VRChanged;

        public override void Start()
        {
            base.Start();
            _VREnabled = Application.Engine.Renderer.VREnabled;
            _UIRectangle = new Rectangle(10, 10, 100, 30);
            _hands = new GameObject[2];
            CreateHand(0);
            CreateHand(1);
            Enabled = false;
        }

        public override void OnDestroy()
        {
            Application.Engine.Renderer.SetVREnabled(false);
        }

        public override void OnEnabled() => SetActive(true);
        public override void OnDisabled() => SetActive(false);

        private void CreateHand(int id)
        {
            _hands[id] = new GameObject($"Hand_{id}");
            _hands[id].AddComponent<MotionController>().LeftHand = id == 0;
            _hands[id].Enabled = false;

#if DESKTOP
            var controller = GameObjectFactory.CreateMesh(GeometryType.Cube);
            controller.Transform.LocalScale = new Vector3(0.1f);
#else
            var cModel = Application.Content.Load<Model>("Models/VRController/vr_controller_01_mrhat");
            var controller = cModel.ToMeshRenderers();

            var renderers = controller.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var std = (StandardMaterial)renderer.Material;
                std.MainTexture = Application.Content.Load<Texture2D>("Models/VRController/vr_controller_01_mrhat_diff_0");
                std.EmissiveMap = Application.Content.Load<Texture2D>("Models/VRController/vr_controller_01_mrhat_Illum");
                std.EmissiveEnabled = true;
            }
#endif

            controller.Transform.Parent = _hands[id].Transform;
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);

            if (Automatic)
                return;

            if (ui.Button(_UIRectangle, "Toggle VR"))
                Toggle();
        }

        private void Toggle()
        {
            _VREnabled = Application.Engine.Renderer.VREnabled;
            _VREnabled = !_VREnabled;

            if (_VREnabled)
                _VREnabled = Application.Engine.Renderer.SetVREnabled(true);

            var parent = Camera.Main.Transform.Parent;

            foreach (var hand in _hands)
            {
                hand.Enabled = _VREnabled;
                if (parent != null)
                    hand.Transform.Parent = parent;
            }

            VRChanged?.Invoke(_VREnabled);
        }

        private void SetActive(bool mustActivate)
        {
            var vrEnabled = Application.Engine.Renderer.VREnabled;

            if (vrEnabled && mustActivate || !vrEnabled && !mustActivate)
                return;

            _VREnabled = Application.Engine.Renderer.SetVREnabled(mustActivate);

            var parent = Camera.Main.Transform.Parent;
            foreach (var hand in _hands)
            {
                hand.Enabled = _VREnabled;
                if (parent != null)
                    hand.Transform.Parent = parent;
            }

            VRChanged?.Invoke(_VREnabled);
        }
    }
}