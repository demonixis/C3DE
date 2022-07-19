using C3DE.Components;
using C3DE.Demo.Scripts.VR;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public sealed class DemoSceneMenu : Behaviour
    {
        private Behaviour[] _behaviours;
        private SideMenu _sideMenu;
        private Vector2 _FPSPosition;

        public override void Start()
        {
            var camera = Camera.Main;

            _behaviours = new Behaviour[]
            {
                camera.GetComponent<ControllerSwitcher>(),
                camera.AddComponent<PostProcessSwitcher>(),
                camera.AddComponent<RendererSwitcher>(),
                camera.AddComponent<VRSwitcher>()
            };

            _sideMenu = new SideMenu(null, new[] { "Controls", "Post Process", "Renderers", "Virtual Reality", "Cancel" }, -1);
            _sideMenu.SelectionChanged += OnSelectionChanged;
            _sideMenu.SetHorizontal(false);

            _FPSPosition = new Vector2(Screen.WidthPerTwo - 5, 15);

            OnSelectionChanged(-1);
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);
            _sideMenu.Draw(ui);
        }

        private void OnSelectionChanged(int item)
        {
            var count = _behaviours.Length;
            if (item >= count)
            {
                foreach (var behaviour in _behaviours)
                    behaviour.Enabled = false;
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    if (_behaviours[i] is VRSwitcher)
                    {
                        if (_behaviours[i].Enabled)
                            continue;
                    }

                    _behaviours[i].Enabled = item == i;
                }
            }
        }
    }
}
