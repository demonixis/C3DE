using C3DE.Components;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public sealed class DemoSceneMenu : Behaviour
    {
        private Behaviour[] m_Behaviours;
        private SideMenu m_SideMenu;
        private Vector2 m_FPSPosition;

        public override void Start()
        {
            var camera = Camera.Main;

            m_Behaviours = new Behaviour[]
            {
                camera.GetComponent<ControllerSwitcher>(),
                camera.AddComponent<PostProcessSwitcher>(),
                camera.AddComponent<RendererSwitcher>(),
                camera.AddComponent<VRSwitcher>()
            };

            m_SideMenu = new SideMenu(null, new[] { "Controls", "Post Process", "Renderers", "Virtual Reality", "Cancel" }, -1);
            m_SideMenu.SelectionChanged += OnSelectionChanged;
            m_SideMenu.SetHorizontal(false);

            m_FPSPosition = new Vector2(Screen.WidthPerTwo - 5, 15);

            OnSelectionChanged(-1);
        }

        public override void OnGUI(GUI ui)
        {
            base.OnGUI(ui);
            m_SideMenu.Draw(ui);

            ui.Label(m_FPSPosition, Application.Engine.FPS.ToString());
        }

        private void OnSelectionChanged(int item)
        {
            var count = m_Behaviours.Length;
            if (item >= count)
            {
                foreach (var behaviour in m_Behaviours)
                    behaviour.Enabled = false;
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    if (m_Behaviours[i] is VRSwitcher)
                    {
                        if (m_Behaviours[i].Enabled)
                            continue;
                    }

                    m_Behaviours[i].Enabled = item == i;
                }
            }
        }
    }
}
