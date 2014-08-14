using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class ControllerSwitcher : Behaviour
    {
        private Camera _camera;
        private OrbitController _orbitController;
        private FirstPersonController _fpController;
        private Rectangle _box;
        private Rectangle _btn1;
        private Rectangle _btn2;
        private Rectangle _checkRect;
        private Rectangle _checkRect2;
        private bool _resetPosition;
        private bool _flyMode;

        public override void Start()
        {
            _camera = GetComponent<Camera>();
            _orbitController = AddComponent<OrbitController>();
            _fpController = AddComponent<FirstPersonController>();
            _fpController.Enabled = false;

            int width = 140;
            int height = 190;

            _box = new Rectangle(Screen.Width - width - 10, Screen.Height - height - 10, width, height);
            _btn1 = new Rectangle(_box.X + 10, _box.Y + 30, _box.Width - 20, 30);
            _btn2 = new Rectangle(_box.X + 10, _btn1.Y + 40, _box.Width - 20, 30);
            _checkRect = new Rectangle(_box.X + 10, _btn2.Y + 40, _box.Width - 20, 30);
            _checkRect2 = new Rectangle(_box.X + 10, _checkRect.Y + 40, _box.Width - 20, 30);
            _resetPosition = true;
            _flyMode = false;

            _orbitController.MaxDistance = 200;
        }

        public void SetControllerActive(int id)
        {
            _orbitController.Enabled = (id == 0) ? true : false;
            _fpController.Enabled = !_orbitController.Enabled;

            if (_fpController.Enabled)
            {
                if (_resetPosition)
                {
                    transform.Position = new Vector3(0, 2, 0);
                    _camera.Target = Vector3.Zero;
                }

                _fpController.Fly = _flyMode;
            }
        }

        public override void OnGUI(GUI gui)
        {
            gui.Box(_box, "Controller");

            if (gui.Button(_btn1, "Orbit"))
                SetControllerActive(0);

            if (gui.Button(_btn2, "First Person"))
                SetControllerActive(1);

            _resetPosition = gui.Checkbox(_checkRect, "Reset position", _resetPosition);
            _flyMode = gui.Checkbox(_checkRect2, "Fly Mode", _flyMode);
        }
    }
}
