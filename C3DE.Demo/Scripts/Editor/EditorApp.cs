using C3DE.Components;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Demo.Scripts.Editor
{
    public class EditorApp : Behaviour
    {
        private MenuBar m_MenuBar;
        private MenuItem m_LastSelected;

        public event Action<string> Clicked = null;

        public override void Start()
        {
            var items = new[]
            {
                new MenuItem("File", OnMainSelected, new [] {
                   new MenuItem("New"),
                   new MenuItem("Save"),
                   new MenuItem("Load"),
                   new MenuItem("Exit", (i) => Application.Quit()),
                }),
                new MenuItem("GameObject", OnMainSelected, new [] {
                    new MenuItem("Cube", OnSecondSelected),
                    new MenuItem("Sphere", OnSecondSelected),
                    new MenuItem("Terrain", OnSecondSelected),
                }),
                new MenuItem("Help",OnMainSelected, new []
                {
                    new MenuItem("About")
                })
            };

            m_MenuBar = new MenuBar(items, GraphicsHelper.CreateTexture(Color.LightGray, 1, 1));
            m_MenuBar.Compute(35, 5);
        }

        private void OnSecondSelected(MenuItem item) => Clicked?.Invoke(item.Header);

        private void OnMainSelected(MenuItem item)
        {
            if (m_LastSelected == item)
            {
                m_LastSelected = null;
                return;
            }

            if (m_LastSelected != null)
                m_LastSelected.Close();

            m_LastSelected = item;
        }

        public override void Update()
        {
            m_MenuBar.Update();
        }

        public override void OnGUI(GUI ui)
        {
            m_MenuBar.Draw(ui);
        }
    }
}
