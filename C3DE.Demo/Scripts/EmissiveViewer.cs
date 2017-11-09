using C3DE.Components;
using C3DE.Graphics.Materials;
using C3DE.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Demo.Scripts
{
    public class EmissiveViewer : Behaviour
    {
        private Rectangle _rect;

        public override void Start()
        {
            _rect = new Rectangle(0, 0, 150, 150);
        }

        public override void OnGUI(GUI gui)
        {
            gui.DrawTexture(_rect, Application.Engine.Renderer.EmissiveTexture);
        }
    }
}
