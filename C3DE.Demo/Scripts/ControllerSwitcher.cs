using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class ControllerSwitcher : Behaviour
    {
        public enum ControllerType
        {
            Orbit, FPS
        }

        private Camera _camera;
        private OrbitController _orbitController;
        private FirstPersonController _fpController;
        private Rectangle _box;
        private Rectangle _btn1;
        private Rectangle _btn2;
        private Rectangle _btn3;
        private Rectangle _checkRect2;
        private bool _flyMode;

        public bool FlyMode
        {
            get => _flyMode;
            set
            {
                _flyMode = value;
                _fpController.Fly = value;
            }
        }

        public Vector3 DefaultPosition { get; set; } = new Vector3(0.0f, 2.0f, 0.0f);
        public Vector3 DefaultRotation { get; set; } = Vector3.Zero;

        public override void Start()
        {
            _camera = GetComponent<Camera>();
            _orbitController = AddComponent<OrbitController>();
            _fpController = AddComponent<FirstPersonController>();
            _fpController.Enabled = false;

            int width = 180;
            int height = 200;

            _box = new Rectangle(Screen.VirtualWidth - width - 10, Screen.VirtualHeight - height - 10, width, height);
            _btn1 = new Rectangle(_box.X + 10, _box.Y + 30, _box.Width - 20, 30);
            _btn2 = new Rectangle(_box.X + 10, _btn1.Y + 40, _box.Width - 20, 30);
            _btn3 = new Rectangle(_box.X + 10, _btn2.Y + 40, _box.Width - 20, 30);
            _checkRect2 = new Rectangle(_box.X + 10, _btn3.Y + 40, _box.Width - 20, 30);

            _orbitController.MaxDistance = 200;

            transform.LocalPosition = DefaultPosition;
            _camera.Target = DefaultRotation;
        }

        public void SetControllerActive(ControllerType type)
        {
            _orbitController.Enabled = type == ControllerType.Orbit;
            _fpController.Enabled = type == ControllerType.FPS;
        }

        public override void OnGUI(GUI gui)
        {
            gui.Box(_box, "Controller");

            if (gui.Button(_btn1, "Orbit"))
                SetControllerActive(ControllerType.Orbit);

            if (gui.Button(_btn2, "First Person"))
                SetControllerActive(ControllerType.FPS);

            if (gui.Button(_btn3, "Reset position"))
            {
                transform.LocalPosition = DefaultPosition;
                _camera.Target = DefaultRotation;
            }

            FlyMode = gui.Checkbox(_checkRect2, "Fly Mode", FlyMode);           
        }
    }
}
